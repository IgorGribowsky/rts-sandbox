---
id: T-013
title: Мусор в коде: лишние using, пустые Update, опечатки в именах
status: todo
milestone: backlog
parent:
origin: ai
needs-design: false
blocked-by: []
mechanics: []
handoff: []
checkpoint:
created: 2026-09-05
updated: 2026-09-05
---

# T-013 · Мусор в коде: лишние using, пустые Update, опечатки в именах

## Что нужно
Мелочи, каждая по отдельности безобидна, вместе мешают читать код.

Лишние `using`, попавшие автодополнением:

- `NavMeshMovement.cs` — `System.Drawing`, `System.Security.Cryptography`,
  `static UnityEngine.UI.Image`;
- `GameObjectExtensions.cs` — `System.Drawing`;
- `ThrowProjectileAction.cs` — `static UnityEngine.UI.GridLayoutGroup`;
- `SelectionBoxController.cs` — `Unity.VisualScripting`.

Пустые `Update()` и `Start()`, которые Unity всё равно вызывает каждый кадр:
`UnitsController`, `SelectionBoxController`, `Selectable`, `TeamMember`,
`GameResources`, `PlayerResources`, `UnitHealthPoints`, `UnitManaPoints`,
`GridSegment`.

Опечатки в именах: `ChechIfCanAddMiner`, `CompletBuilding`, `CancelBulding`,
`CreateMovememtMask`, `prepearedSkill`, `_teamMemeber`, `_resouceValues`,
`barTempalte`. В сцене — объект `SelecionBox`.

Переименование методов безопасно, но переименование **полей** MonoBehaviour
рвёт сериализацию: `_teamMemeber` и `_resouceValues` приватные, так что
на префабах их нет, а вот имя объекта в сцене трогать осторожно.

## Как проверить
Проект компилируется, консоль чистая, смоук-проверка проходит целиком.
Ни один префаб не потерял ссылок в инспекторе.

## План

## Ход работы

## Решения

- 2026-09-05 найдено при `/adopt`. Делать одной задачей и одним коммитом,
  чтобы не размазывать шум по истории.
- 2026-09-05 (ответ в чате) подтверждено к исправлению, помечено как
  лёгкое. Имя объекта `SelecionBox` в сцене трогаю только вместе с
  правкой сцены и только когда сцену не правишь ты.

## Итог
