---
id: M-005
title: Поведения юнита
status: implemented
source:
tasks: [T-021, T-002, T-004, T-026]
---

# M-005 · Поведения юнита

## Зачем она в игре
Юнит в каждый момент делает ровно одно: идёт, бьёт, строит, рубит. Это
правило и держит всю систему простой.

## Как работает

Поведение — обычный C# класс за интерфейсом `IUnitBehaviour`, а не
компонент. На префабе висит один `UnitBehaviourManager`, и его список
`Behaviours` — это и есть ответ на вопрос «что юнит умеет».

### Три части

`UnitBehaviourBase` — общий предок. Даёт `IsActive`, `StartAction(EventArgs)`
и `Tick()`, разбитый на `PreUpdate`, затем `UpdateAction` (только если
`IsActive`), затем `PostUpdate`. Вместо `Awake` у поведения `OnInitialize`,
вместо `OnDestroy` — `Dispose`. Обёртки `gameObject`, `transform` и
`GetComponent<T>()` берут своё из `UnitBehaviourContext` — там владелец и сам
менеджер.

`UnitBehaviourFactory` — единственное место, где тип поведения превращается
в класс. Явный `switch`, а не рефлексия: code stripping в билде выбрасывает
классы, на которые никто не ссылается по имени.

`UnitBehaviourManager` — создаёт поведения по списку с префаба, инициализирует
их вторым проходом (поведение может искать соседей), подписывается на все
события приказов один раз и держит одно текущее поведение. Смена — это
`_current.Deactivate()` и новая ссылка, без перебора списка. Строка
`CurrentBehaviourInfo` показывает текущее поведение в инспекторе, как
`CurrentRunningCommandInfo` у `UnitCommandManager`.

### Как приказ находит поведение

Поведение объявляет `Trigger` — приказ, на который отвечает
(`UnitActionType`), и при необходимости `CanHandle(args)`, если на один приказ
претендует несколько поведений. Менеджер по приказу берёт первое подходящее.
Поэтому новое поведение — это новый класс, значение в `UnitBehaviourType` и
строка в фабрике; менеджер не трогается.

Каст пользуется `CanHandle`: точечный берётся за приказ только если пришли
аргументы каста в точку и действие способности — `CastToPointAction`, а каст
в цель — только на своих аргументах и `CastToTargetAction`. Оба отвечают на
один приказ `SkillCast` и делят общую базу `SkillCastingBehaviourBase`:
таймер каста, проверка возможности каста и завершение там, различаются
только прицелом (T-002).

### Тикают все, работает одно

`Tick()` менеджер зовёт у ВСЕХ поведений юнита каждый кадр, `UpdateAction` —
только у активного. На `PreUpdate`/`PostUpdate` неактивных держится
существенное: сброс кулдауна атаки, выход шахтёра из ячейки при смене
приказа, таймер агрессии автоатаки.

### Автоатака держит боевое поведение

Автоатака остаётся активной сама и параллельно включает боевое поведение
своего же юнита — тот самый экземпляр, который работает по явному приказу
Attack, взятый у менеджера через `GetForAction(UnitActionType.Attack)`.
`Deactivate()` автоатаки гасит и его.

### Список поведений

| Поведение | Приказ | Что делает |
|---|---|---|
| `MovementBehaviour` | Move | идёт в точку, завершается по `StoppingDistance` |
| `AMovementBehaviour` | AMove | идёт в точку, по дороге атакует враждебное |
| `AutoAttackIdleBehaviour` | AutoAttackIdle | стоит и бьёт подошедших, возвращается |
| `AutoAttackBuildingBehaviour` | AutoAttackIdle | башня: бьёт, но никуда не идёт |
| `FollowingBehaviour` | Follow | держится в `FollowingDistance` 1.2 от цели |
| `MeleeAttackingBehaviour` | Attack | ближний бой |
| `RangeAttackingBehaviour` | Attack | стрельба снарядом |
| `HoldingBehaviour` | Hold | останавливает движение и всё |
| `BuildingBehaviour` | Build | доходит и ставит здание |
| `MiningBehaviour` | Mine | встаёт в ячейку шахты |
| `HarvestingBehaviour` | Harvest | рубит и носит на склад |
| `SkillCastingToPointBehaviour` | SkillCast | подходит к точке и кастует |
| `SkillCastingToTargetBehaviour` | SkillCast | подходит к юниту и кастует в него |
| `StunnedBehaviour` | Stun | юнит оглушён и не делает ничего |

Набор на префабах:

| Префаб | Behaviours |
|---|---|
| Warrior, Giant Unit | Movement, AMovement, Following, Holding, MeleeAttacking, AutoAttackIdle, Stunned |
| Range Unit | Movement, AMovement, Following, Holding, RangeAttacking, AutoAttackIdle, Stunned |
| Caster Unit | то же, что Range Unit, плюс SkillCastingToPoint, SkillCastingToTarget и Stunned |
| Builder | Movement, AMovement, Following, Holding, MeleeAttacking, Building, Mining, Harvesting, Stunned |
| Tower | RangeAttacking, AutoAttackBuilding |

`Stunned` есть у всех, кого можно оглушить, и нет у башни: здания отсекает
фильтр целей (M-019).

У Castle, Barracks, Farm, Wall и mine_held `UnitBehaviourManager` нет вовсе.

### Конец действия

`TriggerEndEventFlag` управляет тем, шлёт ли поведение своё `ActionEnded`.
Автоатака вызывает `DisableTriggerEndEvent()` на боевом поведении, чтобы
конец боя не сдвинул очередь команд юнита — это её внутренний бой, а не
приказ игрока.

### Автоатака подробнее

`AutoAttackingBehaviourBase` каждый кадр ищет ближайшего врага в радиусе
`AutoAttackDistance` через `GetNearestUnitInRadius`, который делает
`FindGameObjectsWithTag("Unit")` — то есть перебор всех юнитов сцены каждый
кадр на каждом юните.

`AutoAttackIdleBehaviour` дополнительно помнит точку, где стоял: если
преследование увело дальше `PersecutionDistance` = 50, юнит бросает цель и
возвращается. По зову союзника (M-008) он агрится на 3 секунды
(`DamageReceivedAgressionTime`) и в это время не переключается на других.

## Границы
Поведение не решает, когда его включить, — это `UnitBehaviourManager` по
событию. Поведение не знает про очередь, только шлёт `ActionEnded`.

Снаружи в поведения ходят только `UnitsController` и `UnitCommandManager`, и
только через менеджер: `Has<T>()`, `IsBehaviourActive<T>()`, `Get<T>()`,
`CanHandle(action, args)`. Последний нужен очереди приказов, чтобы не
запускать команду, которую на этом юните исполнить нечем (T-004).

## Открытые места
- Оглушение единственное поведение, которое включается не приказом, а
  эффектом (`StunStarted` от `StunEffect`), и снимается вместе с ним.
  Приказы во время него доходят до очереди, но не запускаются: заслонка
  живёт в `UnitCommandManager`, не здесь (M-019).
- Пустые `IfNoTargetUpdate` и `IfTargetExistsUpdate` у
  `AutoAttackBuildingBehaviour` — башня никуда не идёт, это осознанно.
- Приказ, на который у юнита нет поведения, просто игнорируется. Раньше он
  гасил текущее поведение и не включал ничего. Для приказа каста это
  отдельно закрыто в T-004: очередь спрашивает `CanHandle` заранее и
  выбрасывает команду, вместо того чтобы ждать `ActionEnded` навсегда.
  Для остальных приказов дыра остаётся.
