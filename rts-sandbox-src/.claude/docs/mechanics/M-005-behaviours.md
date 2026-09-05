---
id: M-005
title: Поведения юнита
status: implemented
source:
tasks: []
---

# M-005 · Поведения юнита

> **Переписывается.** Система переводится на паттерн Strategy в T-021 —
> первой задаче версии 0.1.1-alpha. Эта дока описывает, как всё устроено
> сейчас; после T-021 она переписывается целиком.

## Зачем она в игре
Юнит в каждый момент делает ровно одно: идёт, бьёт, строит, рубит. Это
правило и держит всю систему простой.

## Как работает
`UnitBehaviourBase` — общий предок. У него `IsActive`, единый `Update`,
разбитый на `PreUpdate`, затем `UpdateAction` (только если `IsActive`),
затем `PostUpdate`, и метод `StartAction(EventArgs)`.

`UnitBehaviourManager` при `Awake` смотрит, какие поведения реально висят на
префабе, и подписывает только их. Набор поведений на префабе и определяет,
что юнит умеет: у стены нет ни одного, у строителя есть строительство,
добыча и рубка. При старте любого поведения все остальные гасятся
(`UnitBehaviourCases.ForEach(x => x.IsActive = false)`).

Список поведений:

| Поведение | Приказ | Что делает |
|---|---|---|
| `MovementBehaviour` | Move | идёт в точку, завершается по `StoppingDistance` |
| `AMovementBehaviour` | A-move | идёт в точку, по дороге атакует враждебное |
| `AutoAttackIdleBehaviour` | Idle | стоит и бьёт подошедших, возвращается |
| `AutoAttackBuildingBehaviour` | Idle зданий | башня: бьёт, но никуда не идёт |
| `FollowingBehaviour` | Follow | держится в `FollowingDistance` 1.2 от цели |
| `MeleeAttackingBehaviour` | Attack | ближний бой |
| `RangeAttackingBehaviour` | Attack | стрельба снарядом |
| `HoldingBehaviour` | Hold | останавливает движение и всё |
| `BuildingBehaviour` | Build | доходит и ставит здание |
| `MiningBehaviour` | Mine | встаёт в ячейку шахты |
| `HarvestingBehaviour` | Harvest | рубит и носит на склад |
| `SkillCastingToPointBehaviour` | SkillCast | подходит на дальность и кастует |

`TriggerEndEventFlag` управляет тем, шлёт ли поведение своё `ActionEnded`.
Автоатака вызывает `DisableTriggerEndEvent()` на боевом поведении, чтобы
конец боя не сдвинул очередь команд юнита — это её внутренний бой, а не
приказ игрока.

Автоатака (`AutoAttackingBehaviourBase`) каждый кадр ищет ближайшего врага
в радиусе `AutoAttackDistance` через `GetNearestUnitInRadius`, который
делает `FindGameObjectsWithTag("Unit")` — то есть перебор всех юнитов сцены
каждый кадр на каждом юните.

`AutoAttackIdleBehaviour` дополнительно помнит точку, где стоял: если
преследование увело дальше `PersecutionDistance` = 50, юнит бросает цель и
возвращается. По зову союзника (M-008) он агрится на 3 секунды
(`DamageReceivedAgressionTime`) и в это время не переключается на других.

## Границы
Поведение не решает, когда его включить, — это `UnitBehaviourManager` по
событию. Поведение не знает про очередь, только шлёт `ActionEnded`.

## Открытые места
- Оглушение (M-019) добавит поведение, которое включается эффектом и
  снимается вместе с ним, при этом приказы во время него продолжают
  приниматься в очередь.
- Каст в цель (`SkillCastingToTargetBehaviour`) закомментирован в менеджере,
  класса нет.
- `SkillCastActionStarted` подписывается всегда, а отписывается только если
  на объекте есть `SkillCastingToPointBehaviour`.
- Пустые `IfNoTargetUpdate` и `IfTargetExistsUpdate` у
  `AutoAttackBuildingBehaviour` — башня никуда не идёт, это осознанно.
