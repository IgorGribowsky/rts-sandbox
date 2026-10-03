---
id: T-013
title: Мусор в коде: лишние using, пустые Update, опечатки в именах
status: done
milestone: v0.2.0
parent:
origin: ai
needs-design: false
blocked-by: []
mechanics: []
handoff: []
checkpoint: dfcd03c
created: 2026-09-05
updated: 2026-10-04
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

1. Лишние `using` — не по списку из задачи, а перебором всех классических
   «случайных» пространств имён по всему коду.
2. Пустые `Update` и `Start` — тоже перебором, а не по списку.
3. Опечатки в именах методов и приватных полей.
4. Имя объекта `SelecionBox` в сцене — только когда сцену не правит
   пользователь.

## Ход работы

- 2026-09-11 удалены пустые `Update` и `Start` в восьми файлах:
  `Selectable`, `TeamMember`, `UnitHealthPoints`, `GameResources`,
  `PlayerResources`, `SelectionBoxController`, `UnitsController`,
  `UnitManaPoints`. Пустой `Start` у `GridSegment` ушёл раньше, в T-028.
- 2026-09-11 убраны лишние `using`: `System.Drawing`,
  `System.Security.Cryptography` и `static UnityEngine.UI.Image` из
  `NavMeshMovement`, `System.Drawing` из `GameObjectExtensions`,
  `Unity.VisualScripting` из `SelectionBoxController`.
  `static UnityEngine.UI.GridLayoutGroup` в `ThrowProjectileAction`
  оказался уже убран раньше.
- 2026-09-11 переименованы опечатки: `ChechIfCanAddMiner` →
  `CheckIfCanAddMiner`, `CompletBuilding` → `CompleteBuilding`,
  `CancelBulding` → `CancelBuilding`, `CreateMovememtMask` →
  `CreateMovementMask`, `_teamMemeber` → `_teamMember`, `_resouceValues`
  → `_resourceValues`. `prepearedSkill` был исправлен ещё в 0.1.1,
  `barTempalte` — при переписывании `BarsContaining` в T-029.
- 2026-09-11 объект сцены `SelecionBox` переименован в `SelectionBox`.
  На него никто не ссылается по имени — ни код, ни доки, только прямой
  ссылкой из инспектора, — так что переименование безопасно.

### Проверка фактами

- Повторный перебор всего кода: **0** пустых `Update`, `Start`, `Awake`,
  `LateUpdate`, `FixedUpdate`; **0** вхождений старых имён с опечатками;
  **0** «случайных» `using` (`System.Drawing`, `System.Security.*`,
  `Unity.VisualScripting`, `static UnityEngine.*`, `UnityEditor` в
  игровом коде).
- Компиляция чистая, **ни одного предупреждения**.
- Сцена стартует: собрано 103 объекта, 97 с тегом `Unit`, реестр 97,
  консоль пустая.
- Бой после переименований работает: у команды 3 было 66 юнитов и
  28180 HP, стало 63 и 27457; ни одной ошибки в консоли.
- Ни один префаб не потерял ссылок: переименованы только методы и
  приватные поля, которых на префабах нет.

## Решения

- 2026-09-05 найдено при `/adopt`. Делать одной задачей и одним коммитом,
  чтобы не размазывать шум по истории.
- 2026-09-05 (ответ в чате) подтверждено к исправлению, помечено как
  лёгкое. Имя объекта `SelecionBox` в сцене трогаю только вместе с
  правкой сцены и только когда сцену не правишь ты.
- 2026-09-11 (решено ИИ) **искал перебором, а не по списку из задачи.**
  Список составлялся при `/adopt` глазами и мог быть неполным; перебор
  нашёл ровно те же места плюс подтвердил, что больше ничего нет.
- 2026-09-11 (решено ИИ) **публичные имена тоже переименованы.**
  `CheckIfCanAddMiner` публичный, но зовут его только из кода: на
  префабах и в сцене имена методов не сериализуются, ссылок из
  UnityEvent в проекте нет.

## Итог

Мусора не осталось: ни пустых `Update`, ни случайных `using`, ни имён с
опечатками. Объект сцены зовётся `SelectionBox`. Компилятор молчит без
единого предупреждения, бой и старт сцены работают как раньше.

Принято пользователем 2026-10-04 (ответ в чате).
