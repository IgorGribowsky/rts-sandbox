'use strict';
// Ядро пайплайна: чтение и валидация front-matter.
// Ничего не решает и никогда не бросает наружу на битых данных —
// проблемы возвращаются списком, вызывающий их показывает.

const fs = require('fs');
const path = require('path');

const TASK_STATUSES = ['todo', 'in-progress', 'review', 'blocked', 'done', 'cancelled'];
const TASK_OPEN = ['todo', 'in-progress', 'review', 'blocked'];
const TASK_REQUIRED = ['id', 'title', 'status', 'milestone', 'origin', 'needs-design', 'created', 'updated'];
const TASK_OPTIONAL = ['parent', 'blocked-by', 'mechanics', 'handoff', 'checkpoint'];
const LIST_KEYS = new Set(['blocked-by', 'mechanics', 'handoff', 'tasks']);

const MILESTONE_STATUSES = ['current', 'planned', 'released'];
const HANDOFF_STATUSES = ['waiting', 'ready', 'integrated'];
const QUESTION_STATUSES = ['open', 'deferred', 'answered'];

const RE_TASK_ID = /^T-\d{3}(\.\d+)?$/;
const RE_MILESTONE_ID = /^v\d+\.\d+\.\d+$/;
const RE_DATE = /^\d{4}-\d{2}-\d{2}$/;
const RE_SHORT_HASH = /^[0-9a-f]{7,40}$/;

// ---------------------------------------------------------------- инфраструктура

function findRoot(start) {
  let dir = path.resolve(start || process.cwd());
  for (;;) {
    if (fs.existsSync(path.join(dir, '.claude'))) return dir;
    const up = path.dirname(dir);
    if (up === dir) return null;
    dir = up;
  }
}

function readTextSafe(file) {
  try {
    return fs.readFileSync(file, 'utf8').replace(/^\uFEFF/, '');
  } catch {
    return null;
  }
}

function listMd(dir) {
  try {
    return fs.readdirSync(dir)
      .filter((f) => f.toLowerCase().endsWith('.md') && !f.startsWith('_'))
      .sort()
      .map((f) => path.join(dir, f));
  } catch {
    return [];
  }
}

// ---------------------------------------------------------------- front-matter

// Формат жёсткий намеренно: одна пара ключ-значение на строку, скаляры или
// инлайн-списки. Вложенность и многострочные значения не поддерживаются —
// не потому что лень, а чтобы ИИ не мог написать то, что скрипт прочтёт иначе.
function parseFrontMatter(text) {
  const problems = [];
  if (text == null) return { data: {}, body: '', problems: [{ level: 'error', msg: 'файл не читается' }] };

  const lines = text.split(/\r?\n/);
  if (lines[0].trim() !== '---') {
    return { data: {}, body: text, problems: [{ level: 'error', msg: 'нет front-matter: первая строка файла должна быть ---' }] };
  }

  let end = -1;
  for (let i = 1; i < lines.length; i++) {
    if (lines[i].trim() === '---') { end = i; break; }
  }
  if (end === -1) {
    return { data: {}, body: '', problems: [{ level: 'error', msg: 'front-matter не закрыт второй строкой ---' }] };
  }

  const data = {};
  for (let i = 1; i < end; i++) {
    const raw = lines[i];
    if (!raw.trim() || raw.trim().startsWith('#')) continue;

    if (/^\s/.test(raw)) {
      problems.push({ level: 'error', msg: `строка ${i + 1}: отступ в начале строки, вложенность не поддерживается` });
      continue;
    }
    const colon = raw.indexOf(':');
    if (colon === -1) {
      problems.push({ level: 'error', msg: `строка ${i + 1}: нет двоеточия — «${raw.trim()}»` });
      continue;
    }

    const key = raw.slice(0, colon).trim();
    let value = raw.slice(colon + 1).trim();

    if (!/^[a-z][a-z0-9-]*$/.test(key)) {
      problems.push({ level: 'error', msg: `строка ${i + 1}: ключ «${key}» — только латиница в нижнем регистре и дефис` });
      continue;
    }
    if (Object.prototype.hasOwnProperty.call(data, key)) {
      problems.push({ level: 'error', msg: `ключ «${key}» встречается дважды` });
    }

    if (/^".*"$/.test(value) || /^'.*'$/.test(value)) value = value.slice(1, -1);

    if (value.startsWith('[')) {
      if (!value.endsWith(']')) {
        problems.push({ level: 'error', msg: `ключ «${key}»: список не закрыт скобкой` });
        data[key] = [];
        continue;
      }
      data[key] = value.slice(1, -1).split(',').map((s) => s.trim()).filter(Boolean);
    } else if (value === '') {
      data[key] = LIST_KEYS.has(key) ? [] : null;
    } else if (value === 'true' || value === 'false') {
      data[key] = value === 'true';
    } else {
      data[key] = LIST_KEYS.has(key) ? [value] : value;
    }
  }

  return { data, body: lines.slice(end + 1).join('\n'), problems };
}

function section(body, title) {
  const re = new RegExp(`^##\\s+${title}\\s*$`, 'm');
  const m = re.exec(body || '');
  if (!m) return null;
  const rest = body.slice(m.index + m[0].length);
  const next = /^##\s+/m.exec(rest);
  return (next ? rest.slice(0, next.index) : rest).trim();
}

// ---------------------------------------------------------------- задачи

function readTask(file, opts) {
  const { data, body, problems } = parseFrontMatter(readTextSafe(file));
  const t = {
    file,
    name: path.basename(file),
    archived: !!(opts && opts.archived),
    body,
    problems: problems.slice(),
    id: typeof data.id === 'string' ? data.id : null,
    title: typeof data.title === 'string' ? data.title : null,
    status: typeof data.status === 'string' ? data.status : null,
    milestone: typeof data.milestone === 'string' ? data.milestone : null,
    parent: typeof data.parent === 'string' ? data.parent : null,
    origin: typeof data.origin === 'string' ? data.origin : null,
    needsDesign: data['needs-design'] === true,
    blockedBy: Array.isArray(data['blocked-by']) ? data['blocked-by'] : [],
    mechanics: Array.isArray(data.mechanics) ? data.mechanics : [],
    handoff: Array.isArray(data.handoff) ? data.handoff : [],
    checkpoint: typeof data.checkpoint === 'string' ? data.checkpoint : null,
    created: typeof data.created === 'string' ? data.created : null,
    updated: typeof data.updated === 'string' ? data.updated : null,
    raw: data,
    children: [],
  };

  const p = t.problems;
  for (const key of TASK_REQUIRED) {
    const v = data[key];
    if (v === undefined || v === null || v === '') p.push({ level: 'error', msg: `нет обязательного поля «${key}»` });
  }
  for (const key of Object.keys(data)) {
    if (!TASK_REQUIRED.includes(key) && !TASK_OPTIONAL.includes(key)) {
      p.push({ level: 'warn', msg: `лишнее поле «${key}» — скрипт его игнорирует` });
    }
  }
  if (t.id && !RE_TASK_ID.test(t.id)) p.push({ level: 'error', msg: `id «${t.id}» не по формату T-000 или T-000.1` });
  if (t.status && !TASK_STATUSES.includes(t.status)) p.push({ level: 'error', msg: `status «${t.status}» не из списка` });
  if (t.milestone && t.milestone !== 'backlog' && !RE_MILESTONE_ID.test(t.milestone)) {
    p.push({ level: 'error', msg: `milestone «${t.milestone}» — ожидается vX.Y.Z или backlog` });
  }
  if (t.origin && !['user', 'ai'].includes(t.origin)) p.push({ level: 'error', msg: `origin «${t.origin}» — ожидается user или ai` });
  if (data['needs-design'] !== undefined && typeof data['needs-design'] !== 'boolean') {
    p.push({ level: 'error', msg: 'needs-design — только true или false' });
  }
  for (const key of ['created', 'updated']) {
    if (typeof data[key] === 'string' && !RE_DATE.test(data[key])) p.push({ level: 'error', msg: `${key} «${data[key]}» — ожидается ГГГГ-ММ-ДД` });
  }
  if (t.checkpoint && !RE_SHORT_HASH.test(t.checkpoint)) p.push({ level: 'warn', msg: `checkpoint «${t.checkpoint}» не похож на хеш коммита` });
  if (t.status === 'blocked' && t.blockedBy.length === 0) p.push({ level: 'error', msg: 'status blocked, но blocked-by пустой — непонятно, чего ждём' });
  if (t.needsDesign && t.status === 'in-progress') p.push({ level: 'error', msg: 'needs-design: true и status in-progress одновременно — задача не должна быть в работе' });
  if (t.parent && t.id && !t.id.startsWith(t.parent + '.')) p.push({ level: 'error', msg: `id «${t.id}» не согласован с parent «${t.parent}»` });
  if (t.id && (t.id.match(/\./g) || []).length > 1) p.push({ level: 'error', msg: 'глубина подзадач больше одной не поддерживается' });
  if (t.id && !t.name.startsWith(t.id + '-')) p.push({ level: 'warn', msg: `имя файла не начинается с «${t.id}-»` });
  if (t.archived && t.status && TASK_OPEN.includes(t.status)) p.push({ level: 'error', msg: `лежит в done/, а status «${t.status}» — открытый` });
  if (!t.archived && t.status === 'done') p.push({ level: 'error', msg: 'status done, но файл не уехал в tasks/done/' });

  t.invalid = p.some((x) => x.level === 'error');
  return t;
}

function readTasks(root) {
  const dir = path.join(root, '.claude', 'tasks');
  const open = listMd(dir).map((f) => readTask(f, { archived: false }));
  const archived = listMd(path.join(dir, 'done')).map((f) => readTask(f, { archived: true }));
  const all = open.concat(archived);

  const byId = new Map();
  for (const t of all) {
    if (!t.id) continue;
    if (byId.has(t.id)) {
      t.problems.push({ level: 'error', msg: `дубль id: уже занят файлом ${path.basename(byId.get(t.id).file)}` });
      t.invalid = true;
    } else byId.set(t.id, t);
  }
  for (const t of all) {
    if (!t.parent) continue;
    const parent = byId.get(t.parent);
    if (!parent) {
      t.problems.push({ level: 'error', msg: `parent «${t.parent}» не найден` });
      t.invalid = true;
    } else parent.children.push(t);
  }
  for (const t of all) t.children.sort((a, b) => cmpId(a.id, b.id));

  return { all, open, archived, byId };
}

function cmpId(a, b) {
  const pa = String(a || '').split(/[-.]/);
  const pb = String(b || '').split(/[-.]/);
  for (let i = 1; i < Math.max(pa.length, pb.length); i++) {
    const na = parseInt(pa[i] || '0', 10);
    const nb = parseInt(pb[i] || '0', 10);
    if (na !== nb) return na - nb;
  }
  return 0;
}

// Номер задачи выдаёт скрипт, а не ИИ: номера не переиспользуются,
// поэтому считаем по открытым И закрытым задачам сразу.
function nextId(tasks, parent) {
  const ids = tasks.all.map((t) => t.id).filter(Boolean);
  if (!parent) {
    let max = 0;
    for (const id of ids) {
      const m = /^T-(\d{3})$/.exec(id.split('.')[0]);
      if (m) max = Math.max(max, parseInt(m[1], 10));
    }
    return 'T-' + String(max + 1).padStart(3, '0');
  }
  let max = 0;
  for (const id of ids) {
    if (!id.startsWith(parent + '.')) continue;
    max = Math.max(max, parseInt(id.slice(parent.length + 1), 10) || 0);
  }
  return `${parent}.${max + 1}`;
}

// ---------------------------------------------------------------- milestones

function readMilestones(root) {
  const files = listMd(path.join(root, '.claude', 'milestones'));
  const list = files.map((file) => {
    const { data, body, problems } = parseFrontMatter(readTextSafe(file));
    const m = {
      file,
      name: path.basename(file),
      id: typeof data.id === 'string' ? data.id : null,
      title: typeof data.title === 'string' ? data.title : null,
      status: typeof data.status === 'string' ? data.status : null,
      tasks: Array.isArray(data.tasks) ? data.tasks : [],
      released: typeof data.released === 'string' ? data.released : null,
      goal: (body || '').trim(),
      problems: problems.slice(),
    };
    if (!m.id) m.problems.push({ level: 'error', msg: 'нет поля id' });
    else if (!RE_MILESTONE_ID.test(m.id)) m.problems.push({ level: 'error', msg: `id «${m.id}» — ожидается vX.Y.Z` });
    if (!m.status) m.problems.push({ level: 'error', msg: 'нет поля status' });
    else if (!MILESTONE_STATUSES.includes(m.status)) m.problems.push({ level: 'error', msg: `status «${m.status}» не из списка` });
    if (m.id && !m.name.startsWith(m.id)) m.problems.push({ level: 'warn', msg: 'имя файла не совпадает с id' });
    m.invalid = m.problems.some((x) => x.level === 'error');
    return m;
  });
  list.sort((a, b) => String(a.id).localeCompare(String(b.id), 'en', { numeric: true }));
  return list;
}

function current(milestones) {
  return milestones.find((m) => m.status === 'current')
      || milestones.find((m) => m.status === 'planned')
      || null;
}

// Порядок задач в версии — из файла milestone. В front-matter задачи только
// обратная ссылка. Один источник порядка, поэтому «поменяй порядок» — правка
// одного файла, а не десяти.
function tasksOf(milestone, tasks) {
  if (!milestone) return [];
  const out = [];
  for (const id of milestone.tasks) {
    const t = tasks.byId.get(id);
    if (t) out.push(t);
    else out.push({ id, missing: true, title: 'нет файла задачи', status: null, problems: [], children: [] });
  }
  return out;
}

function progress(list) {
  const counted = list.filter((t) => t.status !== 'cancelled' && !t.missing);
  const done = counted.filter((t) => t.status === 'done').length;
  return { done, total: counted.length };
}

function bar(done, total, width) {
  const w = width || 10;
  const filled = total ? Math.round((done / total) * w) : 0;
  return '▓'.repeat(filled) + '░'.repeat(Math.max(0, w - filled));
}

// ---------------------------------------------------------------- handoff

function readHandoffs(root) {
  const dir = path.join(root, '.claude', 'handoff');
  return listMd(dir)
    .filter((f) => /^H-\d{3}/.test(path.basename(f)))
    .map((file) => {
      const { data, body, problems } = parseFrontMatter(readTextSafe(file));
      const h = {
        file,
        id: typeof data.id === 'string' ? data.id : null,
        title: typeof data.title === 'string' ? data.title : null,
        status: typeof data.status === 'string' ? data.status : null,
        blocking: data.blocking === true || data.blocking === 'yes',
        kind: typeof data.kind === 'string' ? data.kind : 'прочее',
        target: typeof data.target === 'string' ? data.target : null,
        task: typeof data.task === 'string' ? data.task : null,
        body: body || '',
        problems: problems.slice(),
      };
      if (!h.id) h.problems.push({ level: 'error', msg: 'нет поля id' });
      if (h.status && !HANDOFF_STATUSES.includes(h.status)) h.problems.push({ level: 'error', msg: `status «${h.status}» не из списка` });
      if (!h.target) h.problems.push({ level: 'warn', msg: 'нет target — некуда смотреть, готов ли ассет' });
      h.delivered = h.target ? hasContent(path.join(root, h.target)) : false;
      h.invalid = h.problems.some((x) => x.level === 'error');
      return h;
    });
}

function hasContent(dir) {
  try {
    return fs.readdirSync(dir).some((f) => f !== '.gitkeep' && !f.endsWith('.meta'));
  } catch {
    return false;
  }
}

// Пользователь кинул файл в целевую папку и ничего никому не сказал —
// это штатный путь по памятке. Статус переводит скрипт, не ИИ.
function refreshHandoffs(root, handoffs) {
  const flipped = [];
  for (const h of handoffs) {
    if (h.status !== 'waiting' || !h.delivered) continue;
    const text = readTextSafe(h.file);
    if (text == null) continue;
    const next = text.replace(/^status:\s*waiting\s*$/m, 'status: ready');
    if (next === text) continue;
    try {
      fs.writeFileSync(h.file, next);
      h.status = 'ready';
      flipped.push(h.id);
    } catch { /* только чтение — не беда, доска покажет как есть */ }
  }
  return flipped;
}

// INDEX.md — производная от файлов H-*, а не второй источник правды.
function writeHandoffIndex(root, handoffs) {
  const file = path.join(root, '.claude', 'handoff', 'INDEX.md');
  const mark = { waiting: '·', ready: '✓', integrated: ' ' };
  const lines = [
    '# Что ждёт от тебя ассетов и решений',
    '',
    'Файл генерируется скриптом board.js. Править руками бессмысленно —',
    'источник это файлы H-*.md рядом.',
    '',
  ];
  if (!handoffs.length) lines.push('Пусто.');
  for (const h of handoffs) {
    lines.push(`- ${mark[h.status] || '?'} ${h.id} · ${h.title || '—'} · ${h.status}${h.blocking ? ' · БЛОКИРУЕТ' : ''}`);
  }
  try { fs.writeFileSync(file, lines.join('\n') + '\n'); } catch { /* не критично */ }
}

// ---------------------------------------------------------------- вопросы

function readQuestions(root) {
  const file = path.join(root, '.claude', 'docs', '06-open-questions.md');
  const text = readTextSafe(file);
  if (text == null) return [];
  const out = [];
  let cur = null;
  for (const line of text.split(/\r?\n/)) {
    const head = /^##\s+(Q-\d+)\s*(?:·\s*(.*))?$/.exec(line.trim());
    if (head) {
      cur = { file, id: head[1], title: (head[2] || '').trim(), status: null, for: null, asked: null, problems: [] };
      out.push(cur);
      continue;
    }
    if (!cur) continue;
    const kv = /^(status|for|asked|answered):\s*(.*)$/.exec(line.trim());
    if (kv) cur[kv[1]] = kv[2].trim() || null;
  }
  for (const q of out) {
    if (q.status && !QUESTION_STATUSES.includes(q.status)) q.problems.push({ level: 'warn', msg: `Q «${q.id}»: status «${q.status}» не из списка` });
  }
  return out;
}

function readSmoke(root) {
  const text = readTextSafe(path.join(root, '.claude', 'docs', '07-smoke-check.md'));
  if (text == null) return [];
  return text.split(/\r?\n/)
    .map((l) => /^\s*-\s*\[( |x|X)\]\s*(.+)$/.exec(l))
    .filter(Boolean)
    .map((m) => ({ checked: m[1].toLowerCase() === 'x', text: m[2].trim() }));
}

// ---------------------------------------------------------------- state.md

function readState(root) {
  const text = readTextSafe(path.join(root, '.claude', 'state.md')) || '';
  const get = (key) => {
    const m = new RegExp(`^${key}:\\s*(.*)$`, 'm').exec(text);
    return m ? m[1].trim() || null : null;
  };
  return { task: get('задача'), step: get('шаг'), checkpoint: get('чекпоинт') };
}

function writeState(root, state) {
  const file = path.join(root, '.claude', 'state.md');
  const body = [
    `задача: ${state.task || '—'}`,
    `шаг: ${state.step || '—'}`,
    `чекпоинт: ${state.checkpoint || '—'}`,
    '',
  ].join('\n');
  fs.writeFileSync(file, body);
}

// ---------------------------------------------------------------- валидация набора

function validate(root) {
  const tasks = readTasks(root);
  const milestones = readMilestones(root);
  const handoffs = readHandoffs(root);
  const questions = readQuestions(root);
  const issues = [];
  const add = (level, where, msg) => issues.push({ level, where, msg });

  for (const t of tasks.all) for (const p of t.problems) add(p.level, path.basename(t.file), p.msg);
  for (const m of milestones) for (const p of m.problems) add(p.level, path.basename(m.file), p.msg);
  for (const h of handoffs) for (const p of h.problems) add(p.level, path.basename(h.file), p.msg);
  for (const q of questions) for (const p of q.problems) add(p.level, '06-open-questions.md', p.msg);

  // ровно одна текущая версия
  const currents = milestones.filter((m) => m.status === 'current');
  if (currents.length > 1) add('error', 'milestones/', `status: current сразу у ${currents.length} версий: ${currents.map((m) => m.id).join(', ')}`);
  if (currents.length === 0 && milestones.length) add('warn', 'milestones/', 'ни одной версии со status: current');

  // ровно одна задача in-progress: на этом держится /resume
  const inProgress = tasks.all.filter((t) => t.status === 'in-progress');
  if (inProgress.length > 1) add('error', 'tasks/', `in-progress сразу у ${inProgress.length} задач: ${inProgress.map((t) => t.id).join(', ')}`);

  // рассинхрон принадлежности версии
  const listedIn = new Map();
  for (const m of milestones) {
    for (const id of m.tasks) {
      if (listedIn.has(id)) add('error', path.basename(m.file), `${id} перечислена и в ${listedIn.get(id)}`);
      else listedIn.set(id, m.id);
      if (!tasks.byId.has(id)) add('error', path.basename(m.file), `перечислена задача ${id}, а файла нет`);
    }
  }
  for (const t of tasks.all) {
    if (!t.id || !t.milestone) continue;
    // Подзадача принадлежность версии НАСЛЕДУЕТ и в списке milestone не
    // перечисляется: иначе она нарисуется дважды — и в списке, и под родителем.
    if (t.parent) {
      if (listedIn.get(t.id)) add('error', t.name, 'подзадача перечислена в milestone — там должен быть только родитель');
      const parent = tasks.byId.get(t.parent);
      if (parent && parent.milestone && parent.milestone !== t.milestone) {
        add('error', t.name, `milestone ${t.milestone}, а у родителя ${parent.milestone}`);
      }
      continue;
    }
    const where = listedIn.get(t.id) || null;
    if (t.milestone === 'backlog') {
      if (where) add('error', t.name, `milestone: backlog, но задача перечислена в ${where}`);
    } else if (!where) {
      add('error', t.name, `заявляет ${t.milestone}, но эта версия её не перечисляет`);
    } else if (where !== t.milestone) {
      add('error', t.name, `заявляет ${t.milestone}, а перечислена в ${where}`);
    }
  }

  // ссылки
  for (const t of tasks.all) {
    for (const b of t.blockedBy) {
      if (/^T-/.test(b) && !tasks.byId.has(b)) add('warn', t.name, `blocked-by ${b}: такой задачи нет`);
      if (/^H-/.test(b) && !handoffs.some((h) => h.id === b)) add('warn', t.name, `blocked-by ${b}: такого handoff нет`);
      if (/^Q-/.test(b) && !questions.some((q) => q.id === b)) add('warn', t.name, `blocked-by ${b}: такого вопроса нет`);
    }
    for (const h of t.handoff) {
      if (!handoffs.some((x) => x.id === h)) add('warn', t.name, `handoff ${h}: файла нет`);
    }
  }
  for (const h of handoffs) {
    if (h.task && !tasks.byId.has(h.task)) add('warn', path.basename(h.file), `task ${h.task}: такой задачи нет`);
  }

  // родители и дети
  for (const t of tasks.all) {
    if (!t.children.length) continue;
    const kids = t.children.filter((c) => c.status !== 'cancelled');
    const allDone = kids.length > 0 && kids.every((c) => c.status === 'done');
    if (t.status === 'done' && !allDone) add('error', t.name, 'родитель done, а не все подзадачи закрыты');
    if (t.status === 'in-progress') add('warn', t.name, 'родитель не берётся в работу — в работу идут подзадачи');
  }

  return { issues, tasks, milestones, handoffs, questions };
}

module.exports = {
  TASK_STATUSES, TASK_OPEN, MILESTONE_STATUSES, HANDOFF_STATUSES,
  findRoot, readTextSafe, parseFrontMatter, section,
  readTask, readTasks, nextId, cmpId,
  readMilestones, current, tasksOf, progress, bar,
  readHandoffs, refreshHandoffs, writeHandoffIndex,
  readQuestions, readSmoke,
  readState, writeState, validate,
};
