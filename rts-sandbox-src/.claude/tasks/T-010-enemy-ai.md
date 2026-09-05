---
id: T-010
title: Нет ИИ противника
status: todo
milestone: backlog
parent:
origin: ai
needs-design: true
blocked-by: [Q-7]
mechanics: [M-009, M-012]
handoff: []
checkpoint:
created: 2026-09-05
updated: 2026-09-05
---

# T-010 · Нет ИИ противника

## Что нужно
Вражеские команды полностью пассивны. Юниты отбиваются автоатакой и зовут
союзников (M-008), но никто не строит, не производит и не нападает.
Стравить армии сейчас можно только вручную, через `TestScenarios`.

Отдельная сложность: ресурсы привязаны к объекту с тегом `PlayerController`,
а в сцене он один, на команде игрока. ИИ с экономикой потребует второго
`PlayerController` со своим `PlayerResources` и `PlayerTeamMember`, и
проверки, что весь код, который ищет `FindGameObjectWithTag(PlayerController)`,
берёт правильный. Таких мест много: `Building`, `HeldMine`, `UnitProducing`,
`GridSegment`, `UnitCommandManager`, `HarvestedResource`.

## Как проверить
Запустить сцену и не трогать мышь: через некоторое время к базе игрока
приходит группа врагов.

## План

## Ход работы

## Решения

- 2026-09-05 найдено при `/adopt`. Вопрос Q-7: волны по таймеру или
  полноценный ИИ с экономикой. Волны обходятся без второго
  `PlayerController`.

## Итог
