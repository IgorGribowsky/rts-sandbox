---
id: T-005
title: A-клик по земле падает при неподвижных юнитах в выделении
status: todo
milestone: backlog
parent:
origin: ai
needs-design: false
blocked-by: []
mechanics: [M-003, M-001]
handoff: []
checkpoint:
created: 2026-09-05
updated: 2026-09-05
---

# T-005 · A-клик по земле падает при неподвижных юнитах в выделении

## Что нужно
`UnitsController.CreateMovememtMask` кладёт в маску построения только
юниты с компонентом `MovementBehaviour`. `OnGroundRightClick` это учитывает
и ходит по `GetMovableSelectedUnits()`, а `OnGroundAClick` перебирает все
`SelectedUnits` и лезет в `SelectedUnitsMovementMask[unit.GetInstanceID()]`.
На юните без `MovementBehaviour` это `KeyNotFoundException`.

По правилам выделения (M-003) здания и юниты в одну группу не попадают,
поэтому воспроизвести можно только юнитом, у которого нет движения, но есть
`Selectable`. Проверить, существует ли такой сейчас, и всё равно закрыть.

## Как проверить
Выделить группу, где есть неподвижный выделяемый объект, нажать `A` и
кликнуть по земле: подвижные идут в атаку, консоль чистая.

## План

## Ход работы

## Решения

- 2026-09-05 найдено при `/adopt` чтением кода.

## Итог
