---
id: T-040
title: MissingReferenceException — в выделении остаётся уничтоженный юнит
status: blocked
milestone: v0.1.1
parent:
origin: user
needs-design: false
blocked-by: [repro]
mechanics: [M-003, M-004, M-008]
handoff: []
checkpoint:
created: 2026-09-07
updated: 2026-09-07
---

# T-040 · MissingReferenceException — в выделении остаётся уничтоженный юнит

## Что нужно
Правый клик по земле валит `MissingReferenceException`: в
`UnitsController.SelectedUnits` лежит уничтоженный `GameObject`, а
`GetMovableSelectedUnits` (`UnitsController.cs:573`) обращается к нему без
проверки.

Стектрейс от пользователя 2026-09-07:

```
MissingReferenceException: The object of type 'UnityEngine.GameObject' has been destroyed
UnitsController+<>c.<GetMovableSelectedUnits>b__43_0 (x)  at UnitsController.cs:574
UnitsController.GetMovableSelectedUnits ()                at UnitsController.cs:573
UnitsController.OnGroundRightClick (point, addToCommandsQueue) at UnitsController.cs:184
WindowsInputController.Update ()                          at WindowsInputController.cs:265
```

## Как проверить
Заполняется, когда будет repro.

## План
Пока нет repro — плана нет. Чинить симптом (навесить проверку на null в
`GetMovableSelectedUnits`) без причины нельзя: уничтоженный объект в
выделении — это утечка, и она полезет в других местах.

## Ход работы

- 2026-09-07 проверено фактами, до правки кода:
  - в консоли **38 одинаковых** `MissingReferenceException` и НИЧЕГО перед
    ними. Значит версия «смерть юнита оборвалась чужим исключением, и
    `OnSelectedUnitDied` не дошёл» не подтверждается;
  - все три места, где уничтожаются выделяемые объекты, событие шлют:
    `UnitHealthPoints.cs:52` (юниты и здания по HP), `Building.cs:107`
    (снос постройки), `HeldMine.cs:56` (шахта). `SelectedUnitDiedHandler`
    делает `SelectedUnits.Remove(args.Dead)`;
  - `HarvestedResource.cs:33` уничтожает объект БЕЗ этого события, но
    дерево не выделяется: в `Tree.prefab` нет компонента `Selectable`
    (проверено по GUID `7c1ed2db...`), значит в `SelectedUnits` попасть
    не может;
  - путей, которые добавляли бы уничтоженный объект в выделение обратно,
    в `ApplySelection` и в боксовом выделении нет;
  - `SelectedUnits` разыменовывается без проверки на уничтоженность
    примерно в 12 местах `UnitsController` (строки 59, 109, 169, 202, 223,
    244, 266, 303, 348, 370, 443, 471, 573).
- 2026-09-07 чтением кода причина НЕ найдена. Нужен repro: задача в
  `blocked` до ответа пользователя.

## Решения

- 2026-09-07 задача заведена по ошибке из чата во время приёмки T-004.
  Отдельной задачей, а не правкой T-004: падение в другом коде
  (выделение), T-004 трогала очередь приказов и события каста. Связь не
  исключена, но и не показана.
- 2026-09-07 симптом не заглушается проверкой на null до выяснения
  причины: по скиллу `bugfix` это ровно то, чего делать нельзя, а
  уничтоженный объект в выделении вылезет и в остальных 11 местах.

## Итог
