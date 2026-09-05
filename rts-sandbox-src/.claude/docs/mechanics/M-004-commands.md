---
id: M-004
title: Приказы и очередь команд
status: implemented
source:
tasks: []
---

# M-004 · Приказы и очередь команд

## Зачем она в игре
Игрок может выстроить цепочку действий заранее: пойти, построить, потом
рубить — и уйти заниматься другим краем карты.

## Как работает
Три слоя, разделённые событиями:

```
ввод -> UnitsController -> UnitEventManager (XxxCommandReceived)
     -> UnitCommandManager (очередь) -> UnitEventManager (XxxActionStarted)
     -> UnitBehaviourManager -> конкретное поведение
     -> UnitEventManager (XxxActionEnded) -> следующая команда
```

`UnitEventManager` висит на каждом юните — это его личная шина событий.
`PlayerEventController` — глобальная шина игрока: курсор, ресурсы, начало и
снос постройки, смерть выделенного, изменения очереди команд.

Команды (`ICommand` с методами `Check()` и `Start()`), все объявлены
вложенными классами внутри `UnitCommandManager`: `MoveCommand`,
`AMoveCommand`, `AttackCommand`, `FollowCommand`, `HoldCommand`,
`BuildCommand`, `MineCommand`, `HarvestingCommand`, `SkillCastCommand`.

Очередь: обычный `Queue<ICommand>`. Приказ без Shift чистит очередь и
текущую команду, приказ с Shift добавляется в хвост. Перед запуском
вызывается `Check()`; если он вернул `false` (цель умерла, точки нет,
маны не хватает) — команда молча выбрасывается и берётся следующая.

Когда очередь опустела, юнит уходит в Idle: `SetIdleState` шлёт
`OnAutoAttackIdleStarted` с текущей позицией, и включается автоатака на
месте (M-007). То есть стоять для боевого юнита — это тоже поведение.

Для отладки в инспекторе видны два поля: `CommandListInfo` (имена команд в
очереди) и `CurrentRunningCommandInfo` (что исполняется сейчас, либо `Idle`).

Отмена: `Esc` шлёт `OnCanceled` каждому выделенному юниту. Сейчас на это
событие подписан только `Building` — отменяется недостроенное здание.

## Границы
Очередь ничего не знает о содержании команд, только о порядке. Логика
исполнения — в поведениях (M-005). Очередь производства юнитов в здании —
отдельная система (M-011), с этой очередью не связана.

## Открытые места
- `OnDestroy` в `UnitCommandManager` не отписывает `SkillCastCommandReceived`
  и `SkillCastActionEnded` — единственные два из девяти.
- `SkillCastingToPointBehaviour` завершает каст событием `OnMoveActionEnded`,
  а не `OnSkillCastActionEnded`. Очередь едет, потому что подписана на оба,
  но событие приходит не то.
- Если у юнита нет поведения под пришедший приказ, `ActionEnded` не придёт
  никогда и очередь встанет насовсем.
