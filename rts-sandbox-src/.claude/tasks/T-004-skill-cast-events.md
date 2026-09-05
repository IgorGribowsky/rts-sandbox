---
id: T-004
title: Каст завершается чужим событием и не отписывается
status: todo
milestone: backlog
parent:
origin: ai
needs-design: false
blocked-by: []
mechanics: [M-004, M-005, M-015]
handoff: []
checkpoint:
created: 2026-09-05
updated: 2026-09-05
---

# T-004 · Каст завершается чужим событием и не отписывается

## Что нужно
Три места, где события каста подключены не так, как остальные:

1. `SkillCastingToPointBehaviour.StopAction` шлёт `OnMoveActionEnded`
   вместо `OnSkillCastActionEnded`. Очередь команд едет только потому, что
   `UnitCommandManager` подписан на оба события.
2. `UnitCommandManager.OnDestroy` не отписывает `SkillCastCommandReceived`
   и `SkillCastActionEnded` — единственные два из девяти пар.
3. `UnitBehaviourManager` подписывает `SkillCastActionStarted` безусловно,
   а отписывает только если на объекте есть `SkillCastingToPointBehaviour`.

Отдельно: если у юнита нет поведения под пришедший приказ каста
(`StartSkillCastingBehaviour` молча ничего не делает, когда действие не
`CastToPointAction`), `ActionEnded` не придёт и очередь команд встанет
навсегда. Нужно завершать команду явно.

## Как проверить
Кастером сделать очередь через Shift: движение, каст, движение. Все три
шага должны отработать по порядку. Убить кастера сразу после каста —
консоль чистая. Смоук-проверка целиком проходит.

## План

## Ход работы

## Решения

- 2026-09-05 найдено при `/adopt` чтением кода, в игре не воспроизводилось.

## Итог
