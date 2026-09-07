#!/usr/bin/env node
'use strict';
// Доска пайплайна. Три режима:
//   node board.js                  интерфейс для человека
//   node board.js --brief          15-25 строк для ИИ, без интерфейса
//   node board.js --validate       проверка формата, код возврата 1 при ошибках
//   node board.js --next-id [T-012]  следующий свободный номер задачи
//
// Скрипт читает только front-matter и не падает на битых файлах:
// сломанное рисуется как ⚠ INVALID с именем файла и причиной.

const fs = require('fs');
const path = require('path');
const { execFile } = require('child_process');
const L = require('./lib/tasks.js');

const ARGS = process.argv.slice(2);
const has = (f) => ARGS.includes(f);

const ROOT = L.findRoot(process.cwd());
if (!ROOT) {
  console.error('Не нашёл папку .claude — запусти скрипт внутри проекта с пайплайном.');
  process.exit(2);
}

const C = (() => {
  const on = process.stdout.isTTY && !process.env.NO_COLOR;
  const w = (code) => (s) => (on ? `\x1b[${code}m${s}\x1b[0m` : String(s));
  return { dim: w(2), bold: w(1), inv: w(7), red: w(31), green: w(32), yellow: w(33), cyan: w(36), gray: w(90) };
})();

const STATUS_MARK = {
  done: '✓', 'in-progress': '▶', review: '★', blocked: '⛔', cancelled: '✗', todo: ' ',
};

function load() {
  const v = L.validate(ROOT);
  L.refreshHandoffs(ROOT, v.handoffs);
  L.writeHandoffIndex(ROOT, v.handoffs);
  v.smoke = L.readSmoke(ROOT);
  v.state = L.readState(ROOT);
  v.cur = L.current(v.milestones);
  return v;
}

// ---------------------------------------------------------------- строки панелей

function taskRow(t, indent) {
  const pad = indent ? '  └ ' : '';
  if (t.missing) return { text: `${pad}⚠ ${t.id}  нет файла задачи`, level: 'error' };
  if (t.invalid) return { text: `${pad}⚠ INVALID  ${t.name}`, level: 'error', file: t.file };

  const flags = [];
  if (t.needsDesign) flags.push('needs-design');
  if (t.origin === 'ai') flags.push('от ИИ');
  if (t.blockedBy.length) flags.push('ждёт ' + t.blockedBy.join(', '));

  let status = t.status;
  if (t.children.length) {
    const kids = t.children.filter((c) => c.status !== 'cancelled');
    status = `${kids.filter((c) => c.status === 'done').length}/${kids.length}`;
  }
  const mark = t.children.length ? '┬' : (STATUS_MARK[t.status] || '?');
  const title = (t.title || '—').slice(0, 34);
  return {
    text: `${pad}${mark} ${t.id.padEnd(8)} ${title.padEnd(35)} ${String(status).padEnd(12)}${flags.length ? C.dim(flags.join(' · ')) : ''}`,
    file: t.file, task: t,
    selectable: !t.children.length && ['todo', 'in-progress', 'review', 'blocked'].includes(t.status),
  };
}

function panelVersion(v) {
  const rows = [];
  if (!v.cur) return [{ text: 'Ни одной версии нет. Собери план: /plan', level: 'warn' }];
  const list = L.tasksOf(v.cur, v.tasks);
  rows.push({ text: C.bold(`ТЕКУЩАЯ ВЕРСИЯ — ${v.cur.id} «${v.cur.title || '—'}»`) });
  if (v.cur.goal) for (const line of v.cur.goal.split('\n').slice(0, 3)) if (line.trim()) rows.push({ text: C.dim('  ' + line.trim()) });
  rows.push({ text: '' });
  if (!list.length) rows.push({ text: C.dim('  задач в версии нет') });
  for (const t of list) {
    rows.push(taskRow(t, false));
    for (const c of t.children || []) rows.push(taskRow(c, true));
  }
  return rows;
}

function panelWaiting(v) {
  const rows = [];
  const decisions = v.questions.filter((q) => q.status !== 'answered');
  const needsDesign = v.tasks.open.filter((t) => t.needsDesign && !t.invalid);
  const assets = v.handoffs.filter((h) => h.status !== 'integrated');
  const review = v.tasks.open.filter((t) => t.status === 'review' && !t.invalid);

  rows.push({ text: C.bold(`РЕШЕНИЯ (${decisions.length + needsDesign.length})`) });
  if (!decisions.length && !needsDesign.length) rows.push({ text: C.dim('  пусто') });
  for (const q of decisions) {
    const why = q.for ? `нужно для ${q.for}` : (q.status === 'deferred' ? 'отложено тобой' : 'ждёт ответа');
    rows.push({ text: `  ${q.id.padEnd(6)} ${(q.title || '—').slice(0, 40).padEnd(41)} ${C.dim(why)}`, file: q.file });
  }
  for (const t of needsDesign) {
    rows.push({ text: `  ${t.id.padEnd(6)} ${(t.title || '—').slice(0, 40).padEnd(41)} ${C.dim('needs-design')}`, file: t.file });
  }

  rows.push({ text: '' });
  rows.push({ text: C.bold(`АССЕТЫ (${assets.length})`) });
  if (!assets.length) rows.push({ text: C.dim('  пусто') });
  for (const h of assets) {
    const state = h.status === 'ready' ? C.green('ГОТОВО, внедрить') : (h.blocking ? C.red('ждёт · БЛОКИРУЕТ') : 'ждёт');
    rows.push({ text: `  ${String(h.id).padEnd(6)} ${(h.title || '—').slice(0, 40).padEnd(41)} ${state}`, file: h.file, dir: h.target ? path.join(ROOT, h.target) : null });
  }

  rows.push({ text: '' });
  rows.push({ text: C.bold(`ПРИЁМКА (${review.length})`) });
  if (!review.length) rows.push({ text: C.dim('  пусто') });
  for (const t of review) {
    const age = daysSince(t.updated);
    const note = age >= 3 ? C.yellow(`играй и проверь · ${age} дн.`) : 'играй и проверь';
    rows.push({ text: `  ${t.id.padEnd(6)} ${(t.title || '—').slice(0, 40).padEnd(41)} ${note}`, file: t.file, task: t });
  }
  return rows;
}

function panelPlan(v) {
  const rows = [];
  const future = v.milestones.filter((m) => m !== v.cur && m.status !== 'released');
  for (const m of future) {
    const list = L.tasksOf(m, v.tasks);
    const p = L.progress(list);
    rows.push({ text: C.bold(`${m.id} «${m.title || '—'}»`) + C.dim(`  ${p.done}/${p.total}`) , file: m.file });
    for (const t of list) rows.push(taskRow(t, true));
    rows.push({ text: '' });
  }
  const backlog = v.tasks.open.filter((t) => t.milestone === 'backlog');
  const byUser = backlog.filter((t) => t.origin !== 'ai');
  const byAi = backlog.filter((t) => t.origin === 'ai');

  rows.push({ text: C.bold(`БЭКЛОГ (${byUser.length})`) });
  if (!byUser.length) rows.push({ text: C.dim('  пусто') });
  for (const t of byUser) rows.push(taskRow(t, true));
  rows.push({ text: '' });
  rows.push({ text: C.bold(`НАШЁЛ ИИ, ТЫ ЭТОГО НЕ ЗАКАЗЫВАЛ (${byAi.length})`) });
  if (!byAi.length) rows.push({ text: C.dim('  пусто') });
  for (const t of byAi) rows.push(taskRow(t, true));
  return rows;
}

function panelSmoke(v) {
  const file = path.join(ROOT, '.claude', 'docs', '07-smoke-check.md');
  if (!v.smoke.length) return [{ text: C.dim('Смоук-чека пока нет. Он наполняется при закрытии важных задач.'), file }];
  const rows = [{ text: C.bold(`СМОУК-ЧЕК — ${v.smoke.length} пунктов`) }, { text: '' }];
  for (const s of v.smoke) rows.push({ text: `  [${s.checked ? 'x' : ' '}] ${s.text}`, file });
  if (v.smoke.length > 15) {
    rows.push({ text: '' });
    rows.push({ text: C.yellow('  Больше 15 пунктов — список перестаёт прогоняться за две минуты.') });
  }
  return rows;
}

function panelHistory(v) {
  const done = v.tasks.archived.slice().sort((a, b) => String(b.updated).localeCompare(String(a.updated)) || L.cmpId(b.id, a.id));
  if (!done.length) return [{ text: C.dim('Закрытых задач пока нет.') }];
  const rows = [{ text: C.bold(`ИСТОРИЯ — ${done.length} закрытых задач`) }, { text: '' }];
  for (const t of done) {
    const mark = t.status === 'cancelled' ? '✗' : '✓';
    rows.push({ text: `  ${mark} ${String(t.updated || '—').padEnd(11)} ${String(t.id).padEnd(8)} ${(t.title || '—').slice(0, 40)}`, file: t.file, task: t });
  }
  return rows;
}

const PANELS = [
  { key: '1', name: 'Версия', build: panelVersion },
  { key: '2', name: 'Ждёт тебя', build: panelWaiting },
  { key: '3', name: 'План', build: panelPlan },
  { key: '4', name: 'Смоук', build: panelSmoke },
  { key: '5', name: 'История', build: panelHistory },
];

function daysSince(date) {
  if (!date) return 0;
  const t = Date.parse(date + 'T00:00:00Z');
  if (Number.isNaN(t)) return 0;
  return Math.max(0, Math.floor((Date.now() - t) / 86400000));
}

function waitingCount(v) {
  return v.questions.filter((q) => q.status !== 'answered').length
       + v.tasks.open.filter((t) => t.needsDesign).length
       + v.handoffs.filter((h) => h.status !== 'integrated').length
       + v.tasks.open.filter((t) => t.status === 'review').length;
}

// ---------------------------------------------------------------- --brief

function brief(v) {
  const out = [];
  const errors = v.issues.filter((i) => i.level === 'error');
  const list = L.tasksOf(v.cur, v.tasks);
  const p = L.progress(list);

  out.push(`ВЕРСИЯ: ${v.cur ? `${v.cur.id} «${v.cur.title || '—'}» ${p.done}/${p.total}` : 'нет'}`);
  out.push(`STATE: задача ${v.state.task || '—'} · шаг ${v.state.step || '—'} · чекпоинт ${v.state.checkpoint || '—'}`);

  const active = v.tasks.all.filter((t) => t.status === 'in-progress');
  if (active.length) out.push(`АВАРИЙНО: in-progress ${active.map((t) => t.id).join(', ')} — был обрыв, точка входа для /resume`);

  out.push('ЗАДАЧИ ВЕРСИИ:');
  for (const t of list) {
    if (t.missing) { out.push(`  ⚠ ${t.id} нет файла`); continue; }
    if (t.invalid) { out.push(`  ⚠ INVALID ${t.name}`); continue; }
    const flags = [t.needsDesign ? 'needs-design' : null, t.origin === 'ai' ? 'origin:ai' : null,
      t.blockedBy.length ? 'ждёт ' + t.blockedBy.join(',') : null].filter(Boolean);
    const kids = t.children.filter((c) => c.status !== 'cancelled');
    const st = kids.length ? `${kids.filter((c) => c.status === 'done').length}/${kids.length} подзадач` : t.status;
    out.push(`  ${t.id} ${t.title} — ${st}${flags.length ? ' [' + flags.join(' ') + ']' : ''}`);
  }

  const review = v.tasks.open.filter((t) => t.status === 'review');
  const decisions = v.questions.filter((q) => q.status !== 'answered');
  const ready = v.handoffs.filter((h) => h.status === 'ready');
  const waitingAssets = v.handoffs.filter((h) => h.status === 'waiting');
  out.push(`ЖДЁТ ПОЛЬЗОВАТЕЛЯ: приёмка ${review.length}${review.length ? ' (' + review.map((t) => `${t.id}/${daysSince(t.updated)}дн`).join(', ') + ')' : ''}` +
           ` · решения ${decisions.length}${decisions.length ? ' (' + decisions.map((q) => q.id).join(', ') + ')' : ''}` +
           ` · ассеты готовы ${ready.length}${ready.length ? ' (' + ready.map((h) => h.id).join(', ') + ')' : ''} · ассеты ждут ${waitingAssets.length}`);

  const stale = review.filter((t) => daysSince(t.updated) >= 3);
  if (stale.length) out.push(`НАПОМНИТЬ: приёмка висит ${Math.max(...stale.map((t) => daysSince(t.updated)))} дн. — попроси плейтест`);

  const backlog = v.tasks.open.filter((t) => t.milestone === 'backlog');
  out.push(`БЭКЛОГ: ${backlog.length} (от ИИ ${backlog.filter((t) => t.origin === 'ai').length})`);
  out.push(`СМОУК: ${v.smoke.length} пунктов`);
  out.push(`СЛЕДУЮЩИЙ ID: ${L.nextId(v.tasks, null)}`);
  if (errors.length) {
    out.push(`ФОРМАТ БИТ: ${errors.length} ошибок — прогони board.js --validate и исправь`);
    for (const e of errors.slice(0, 5)) out.push(`  ${e.where}: ${e.msg}`);
  }
  return out.join('\n');
}

// ---------------------------------------------------------------- --validate

function validateOut(v) {
  const errors = v.issues.filter((i) => i.level === 'error');
  const warns = v.issues.filter((i) => i.level === 'warn');
  const lines = [];
  for (const e of errors) lines.push(`ОШИБКА  ${e.where}: ${e.msg}`);
  for (const w of warns) lines.push(`предупр ${w.where}: ${w.msg}`);
  lines.push('');
  lines.push(`Итого: ${errors.length} ошибок, ${warns.length} предупреждений. Задач: ${v.tasks.all.length}, версий: ${v.milestones.length}.`);
  console.log(lines.join('\n'));
  process.exit(errors.length ? 1 : 0);
}

// ---------------------------------------------------------------- интерфейс

function openInEditor(target) {
  if (!target) return;
  try {
    if (process.platform === 'win32') execFile('cmd', ['/c', 'start', '', target], () => {});
    else if (process.platform === 'darwin') execFile('open', [target], () => {});
    else execFile('xdg-open', [target], () => {});
  } catch { /* открыть не вышло — не повод падать */ }
}

function tui() {
  let v = load();
  let panel = 0;
  let cursor = 0;
  let flash = '';
  const firstMovable = (rows) => {
    for (let i = 0; i < rows.length; i++) if (rows[i].file || rows[i].dir || rows[i].task) return i;
    return 0;
  };

  const width = Math.min(Math.max(process.stdout.columns || 80, 64), 100);
  const height = () => Math.max((process.stdout.rows || 24) - 10, 6);

  const rowsCache = () => PANELS[panel].build(v);

  function draw() {
    const rows = rowsCache();
    const project = projectName();
    const list = L.tasksOf(v.cur, v.tasks);
    const p = L.progress(list);
    const head = ` ${project} `;
    const right = v.cur ? ` ${v.cur.id}  ${L.bar(p.done, p.total, 10)}  ${p.done}/${p.total} ` : ' версий нет ';
    const line = '─'.repeat(Math.max(0, width - 2 - head.length - right.length));

    const tabs = PANELS.map((x, i) => {
      const label = `[${x.key}] ${x.name}${i === 1 ? ` (${waitingCount(v)})` : ''}`;
      return i === panel ? C.inv(label) : label;
    }).join('  ');

    const errs = v.issues.filter((i) => i.level === 'error').length;
    const out = [];
    out.push('──' + head + line + right + '──');
    out.push(' ' + tabs);
    if (errs) out.push(' ' + C.red(`⚠ формат бит: ${errs} ошибок — node .claude/scripts/board.js --validate`));
    out.push('');

    const max = height();
    const visible = rows.slice(Math.max(0, Math.min(cursor - max + 3, rows.length - max)), Math.max(0, Math.min(cursor - max + 3, rows.length - max)) + max);
    const offset = Math.max(0, Math.min(cursor - max + 3, rows.length - max));
    visible.forEach((r, i) => {
      const idx = offset + i;
      const isCur = idx === cursor;
      out.push((isCur ? C.cyan('▸ ') : '  ') + (isCur ? C.bold(r.text) : r.text));
    });

    out.push('');
    out.push('─'.repeat(width));
    out.push(C.dim(' ↑↓ выбрать · Enter взять/открыть · E файл · R обновить · 1-5 панели · Q выход'));
    if (flash) out.push(' ' + C.green(flash));

    process.stdout.write('\x1b[2J\x1b[H' + out.join('\n') + '\n');
  }

  function movable(rows, from, dir) {
    for (let i = from + dir; i >= 0 && i < rows.length; i += dir) {
      if (rows[i].text.trim() && (rows[i].file || rows[i].dir || rows[i].task)) return i;
    }
    return from;
  }

  function activate() {
    const rows = rowsCache();
    const r = rows[cursor];
    if (!r) return;
    if (r.task && r.selectable) {
      L.writeState(ROOT, { task: r.task.id, step: 'выбрана на доске, ждёт /next', checkpoint: r.task.checkpoint });
      flash = `${r.task.id} записана в state.md — теперь в чате /next`;
      v = load();
      return;
    }
    if (r.dir) {
      try { fs.mkdirSync(r.dir, { recursive: true }); } catch { /* пусть */ }
      openInEditor(r.dir);
      flash = `Открыл папку ${path.relative(ROOT, r.dir)} — положи файл и всё`;
      return;
    }
    if (r.file) { openInEditor(r.file); flash = `Открыл ${path.basename(r.file)}`; return; }
    flash = 'Тут нечего брать в работу';
  }

  cursor = firstMovable(rowsCache());
  process.stdin.setRawMode(true);
  process.stdin.resume();
  process.stdin.setEncoding('utf8');
  draw();

  // Терминал присылает несколько нажатий одним куском (и стрелку тремя
  // байтами), поэтому чанк разбираем на отдельные клавиши.
  function tokenize(chunk) {
    const keys = [];
    for (let i = 0; i < chunk.length; i++) {
      if (chunk[i] === '\u001b' && chunk[i + 1] === '[') { keys.push(chunk.slice(i, i + 3)); i += 2; }
      else keys.push(chunk[i]);
    }
    return keys;
  }

  process.stdin.on('data', (chunk) => {
    for (const key of tokenize(chunk)) handleKey(key);
    draw();
  });

  function handleKey(key) {
    flash = '';
    const rows = rowsCache();
    if (key === 'q' || key === 'Q' || key === '\u0003') {
      process.stdout.write('\x1b[2J\x1b[H');
      process.exit(0);
    } else if (key >= '1' && key <= '5') {
      panel = Number(key) - 1; cursor = firstMovable(rowsCache());
    } else if (key === '\u001b[A' || key === 'k') {
      cursor = movable(rows, cursor, -1);
    } else if (key === '\u001b[B' || key === 'j') {
      cursor = movable(rows, cursor, 1);
    } else if (key === '\r' || key === '\n') {
      activate();
    } else if (key === 'e' || key === 'E') {
      const r = rows[cursor];
      if (r && r.file) { openInEditor(r.file); flash = `Открыл ${path.basename(r.file)}`; }
    } else if (key === 'r' || key === 'R') {
      v = load(); flash = 'Обновил';
    }
  }
}

function projectName() {
  const text = L.readTextSafe(path.join(ROOT, 'CLAUDE.md')) || '';
  const m = /^#\s+(.+)$/m.exec(text);
  return (m ? m[1].trim() : path.basename(ROOT)).slice(0, 30);
}

// ---------------------------------------------------------------- вход

const v = load();
if (has('--validate')) validateOut(v);
else if (has('--brief')) console.log(brief(v));
else if (has('--next-id')) {
  const parent = ARGS[ARGS.indexOf('--next-id') + 1];
  console.log(L.nextId(v.tasks, parent && /^T-\d{3}$/.test(parent) ? parent : null));
} else if (!process.stdin.isTTY) {
  console.log(brief(v));
  console.log('\n(не TTY — интерфейс не рисую, отдал --brief)');
} else {
  tui();
}
