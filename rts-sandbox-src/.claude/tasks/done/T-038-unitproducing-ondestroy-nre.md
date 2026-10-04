---
id: T-038
title: NullReferenceException в UnitProducing.OnDestroy у недостроенного здания
status: done
milestone: v0.3.1
parent:
origin: user
needs-design: false
blocked-by: []
mechanics: [M-010, M-011]
handoff: []
checkpoint: 893d50c
created: 2026-09-06
updated: 2026-10-04
---

# T-038 · NullReferenceException в UnitProducing.OnDestroy у недостроенного здания

## Что нужно

```
NullReferenceException: Object reference not set to an instance of an object
UnitProducing.OnDestroy () (at Assets/Scripts/GameObjects/UnitProducing.cs:161)
```

Строка 161 — `_unitEventManager.ProduceCommandReceived -= ProduceCommandHandler;`,
и `_unitEventManager` там null.

Причина в связке двух мест:

- `UnitProducing` подписывается в `Start` и там же берёт все свои ссылки
  (`UnitProducing.cs:30-41`);
- `Building.Build()` сразу гасит компонент — `_unitProducing.enabled = false`
  (`Building.cs:55`), а Unity не вызывает `Start` у выключенного компонента.

Значит, у здания, которое ещё строится, `Start` не отработал ни разу. Уничтожь
такое здание — отмени стройку (`Building.CancelBulding`) или добей его — и
`OnDestroy` снимает подписку по null-ссылке.

Достроенное здание не задето: `CompletBuilding()` включает компонент обратно,
`Start` отрабатывает.

Фикс просится один: `OnDestroy` должен переживать невыполненный `Start` —
проверка на null перед отпиской. Заодно посмотреть, нет ли той же формы у
других компонентов, которые `Building.Build()` выключает:
`UnitCommandManager` (подписка в `Awake`, а `Awake` у выключенного компонента
всё же вызывается — вероятно, цел) и `HarvestedResourcesStorage`.

## Как проверить
Построить казарму и отменить стройку до её завершения. Консоль чистая.
Затем то же самое, но здание добить в процессе стройки.

## План

- [x] Repro в Play: недостроенная казарма, отмена и добивание
- [x] Найти, где форма бага ушла

## Ход работы

## Решения

- 2026-09-06 найдено пользователем на плейтесте T-021. К T-021 отношения не
  имеет: ни `UnitProducing`, ни `Building`, ни `BuildingController` в той
  задаче не менялись, баг лежит с момента появления отмены стройки.

- 2026-10-04 кода не менял: баг устранён в T-028 (коммит `ff9fd62`) —
  ссылки `UnitProducing` берутся в `Awake`, подписки в `OnEnable`/`OnDisable`.
  `Awake` у выключенного компонента вызывается, `OnDisable` у ни разу не
  включённого — нет, так что отписки по null больше не бывает. Та же форма у
  `UnitCommandManager`, `HarvestedResourcesStorage`, `Building`,
  `UnitHealthPoints` — везде Awake + OnEnable/OnDisable, `OnDestroy` с
  отписками не осталось.

## Итог

Repro в Play через код: две казармы `Build()`, одной `OnCanceled`, второй
урон 100000 — обе уничтожены, консоль без ошибок и предупреждений.
