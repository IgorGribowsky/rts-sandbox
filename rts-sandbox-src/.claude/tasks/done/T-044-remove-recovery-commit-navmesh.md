---
id: T-044
title: Удалить Assets/_Recovery и закоммитить перезапечённый NavMesh
status: done
milestone: backlog
parent:
origin: user
needs-design: false
blocked-by: []
mechanics: []
handoff: []
checkpoint: a287541
created: 2026-10-04
updated: 2026-10-04
---

# T-044 · Удалить Assets/_Recovery и закоммитить перезапечённый NavMesh

## Что нужно
Заведена задним числом: работа сделана по прямой просьбе в чате.

- `Assets/_Recovery/0.unity` — папка от восстановления проекта, остаток
  Q-3. После T-027 она ссылалась на удалённые префабы юнитов.
- Два ассета NavMesh `SampleScene` изменились в редакторе без задачи
  (Unity перезапекла навмеш) и висели незакоммиченными.

## Как проверить
Папки `Assets/_Recovery` в проекте нет, консоль после обновления базы
ассетов чистая; `SampleScene` стартует, юниты ходят.

## План
1. `git rm` папки вместе с `.meta`.
2. Закоммитить два ассета NavMesh как есть.

## Ход работы

- 2026-10-04 чекпоинт a287541. `Assets/_Recovery/0.unity`,
  `0.unity.meta` и `_Recovery.meta` удалены. Ссылок на них из кода и
  настроек нет: в Build Settings только `SampleScene`.
- 2026-10-04 закоммичены `NavMesh-NavMesh Surface large.asset` и
  `NavMesh-NavMesh Surface small.asset`.

## Решения

- 2026-10-04 (ответ в чате) «_Recovery удаляй, навмеш закоммить».
  Закрывает остаток Q-3.

## Итог
`_Recovery` удалена, перезапечённый NavMesh в репозитории. Принято
пользователем 2026-10-04 (ответ в чате).
