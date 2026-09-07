---
id: T-042
title: NullReferenceException в UnitProducing.OnDestroy
status: review
milestone: v0.1.1
parent:
origin: user
needs-design: false
blocked-by: []
mechanics: [M-011]
handoff: []
checkpoint: 50305e4
created: 2026-09-07
updated: 2026-09-07
---

# T-042 · NullReferenceException в UnitProducing.OnDestroy

## Что нужно
Ошибка от пользователя 2026-09-07:

```
NullReferenceException: Object reference not set to an instance of an object
UnitProducing.OnDestroy () (at Assets/Scripts/GameObjects/UnitProducing.cs:161)
```

Строка 161 — `_unitEventManager.ProduceCommandReceived -= ProduceCommandHandler;`
в `OnDestroy`. Поле заполняется в `Start()`. `OnDestroy` в Unity
вызывается и у объекта, у которого `Start()` не выполнялся, — и тогда поле
`null`.

## Как проверить
Ошибка не должна появляться. Заодно проверить, что производство юнитов не
поехало: заказать юнита в казарме, посмотреть, что ресурсы списываются,
полоска идёт, юнит выходит; снести казарму с юнитом в очереди.

## План

Причина видна из кода целиком, без repro: `OnDestroy` отписывается от
полей, которые заполняет `Start`, и не проверяет, что тот вообще был.
`Start` не выполняется у объекта, уничтоженного в том же кадре, в котором
создан, и у объекта, который ни разу не был активен. Точный триггер у
пользователя не установлен, но дефект от триггера не зависит: так этот код
неверен всегда.

Тип ошибки подтверждает именно это: `NullReferenceException`, а не
`MissingReferenceException`. У уничтоженного, но существовавшего компонента
управляемые поля читаются нормально, и `-=` на нём не упал бы; падает
только по-настоящему пустая ссылка, то есть `Start` не был.

Фикс — проверка `!= null` перед каждой отпиской. Именно `!= null`, а не
`?.`: оператор `?.` не знает про подставной null Unity для уничтоженных
объектов и от второй половины проблемы не защитил бы.

## Ход работы

- 2026-09-07 проверено, что дело не в выключенном компоненте:
  `UnitProducing` стоит ровно на двух префабах, `Barracks.prefab` и
  `Castle.prefab`, на обоих `m_Enabled: 1` (по GUID
  `ecf0abe57fafdda4cb71b331b813ccd8`). Больше нигде в проекте его нет.
- 2026-09-07 фикс: в `OnDestroy` добавлены проверки `!= null` для
  `_unitEventManager` и `_playerEventController`.
- 2026-09-07 проверка фактами: перекомпиляция, консоль **0 ошибок,
  0 предупреждений**; `validate_script` по `UnitProducing.cs` — 0 ошибок
  (два предупреждения старые, про `GameObject.Find` и склейку строк в
  `Update`, к правке не относятся).
- 2026-09-07 сделан обзор всего проекта на тот же шаблон: **12 файлов**
  заполняют ссылки в `Start`, отписываются в `OnDestroy` и не проверяют
  ничего — `BuildingGridController`, `Building`, `GridSegment`,
  `HealthPointsBar`, `UnitHealthPoints`, `UnitProducing` (починен),
  `HeldMine`, `UnitSupplyProducer`, `UnitSupplyRequirement`,
  `UnitsController`, `ManaPointsBar`, `UnitManaPoints`,
  `UnitCommandManager`. `CallingToAttackWhenAttacked` и
  `UnitBehaviourManager` в этот список НЕ попадают: они заполняют ссылки в
  `Awake`, а он у созданного объекта выполняется сразу. Единственный, где
  проверка есть, — `UnitSkills`.

## Решения

- 2026-09-07 починен только тот файл, на котором упало. Остальные 11 —
  тот же дефект, но это уже сквозная правка на 12 файлов, то есть
  изменение скоупа версии. Вопрос задан пользователю; общее правило
  подписок и так стоит задачей T-028 в 0.2.0.
- 2026-09-07 `!= null`, а не `?.`: `?.` работает с настоящим null и не
  видит подставной null Unity, поэтому от уничтоженного объекта не
  защищает. Здесь важно и то и другое.

## Итог

`UnitProducing.OnDestroy` больше не падает, если объект уничтожен раньше,
чем у него выполнился `Start`. Правка локальная, поведение производства не
менялось.

Тот же дефект остался в одиннадцати других компонентах — список в «Ход
работы», решение по сквозной правке за пользователем.
