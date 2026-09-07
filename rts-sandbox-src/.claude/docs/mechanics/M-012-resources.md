---
id: M-012
title: Ресурсы и снабжение
status: implemented
source:
tasks: []
---

# M-012 · Ресурсы и снабжение

## Зачем она в игре
Три ресурса задают три разных дела: золото копают в шахте, дерево рубят
на карте, еда — не запас, а лимит на размер армии.

## Как работает
`ResourceName`: `None`, `Gold`, `Wood`, `Food`.
`ResourceType`: `MiningResource`, `HarvestingResource`, `SupplyResource`.

Связка задана в `GameResources` на GameController:

| Ресурс | Тип | Как берётся |
|---|---|---|
| Gold | MiningResource | шахта, M-013 |
| Wood | HarvestingResource | деревья, M-014 |
| Food | SupplyResource | лимит, не копится |

`PlayerResources` на объекте игрока держит два списка: `ResourcesAmount`
(что есть) и `MaxSupplyResourcesAmount` (потолок по снабжению).
Стартовые значения в сцене: **золото 300000, дерево 100000, еда 0,
потолок еды 10**. Это отладочные числа песочницы, не баланс.

Проверки перед тратой:

- `CheckIfCanSpendResources` — хватает ли обычных ресурсов. Для ресурсов
  типа `SupplyResource` всегда возвращает true, они здесь не проверяются.
- `CheckIfHaveSupply` — для `SupplyResource` проверяет
  `текущее + требуемое <= потолок`.
- `SpendResources` списывает всё, кроме `SupplyResource`: еда не тратится,
  она занимается.

Занятие и освобождение снабжения — два компонента поверх общей базы
`UnitSupplyBase`:

- `UnitSupplyRequirement` на юните: в `Start` прибавляет свою еду из
  `ResourceCost` к занятому, на смерть — вычитает.
- `UnitSupplyProducer` на здании: прибавляет `SupplyResourceProduces` к
  потолку. Если здание ещё строится, ждёт события `BuildingCompleted`.
  Единственный производитель еды — Farm, +6 к потолку.

Любое изменение шлёт `PlayerEventController.OnResourceChanged` со старым
и новым значением. На это событие подписано производство юнитов (M-011).

## Границы
Интерфейса ресурсов нет вообще: значения видны только в инспекторе
`PlayerResources`. Никакого HUD-счётчика в игре не рисуется.

## Открытые места
- Кошелёк один на объект `PlayerController`, и в сцене он один. У команд
  2, 3 и 99 ресурсов нет, поэтому ИИ-противник ими пользоваться не может.
- `HarvestedResourcesStorage.CheckIfCanStore` не вызывается нигде.
- Ни `AddResource`, ни `RemoveResource` не проверяют, что ресурс найден:
  `FirstOrDefault(...)` с последующим обращением к полю.
