---
id: T-002
title: Каст способности в юнита не реализован
status: done
milestone: v0.1.1
parent:
origin: ai
needs-design: false
blocked-by: []
mechanics: [M-015, M-005]
handoff: []
checkpoint: 0cee0e7
created: 2026-09-05
updated: 2026-09-07
---

# T-002 · Каст способности в юнита не реализован

## Что нужно
Способности с целью-юнитом: выбрал скилл, кликнул по юниту, кастер подошёл
на дальность и применил. Сейчас в проекте есть только каст в точку.
Заготовки: интерфейс `ITargetSelected` в `TargetedSkillAction.cs`,
закомментированный `SkillCastingToTargetBehaviour` в
`UnitBehaviourManager.cs:115`, поле `SkillParams.Target`.

## Как проверить
Завести способность с целью на Caster Unit, нажать её клавишу и кликнуть по
врагу: кастер подходит, кастует, урон проходит, мана списывается.
Клик по союзнику при `TargetType = Enemies` каст не запускает.

## План

Каст в юнита встаёт рядом с кастом в точку тем же способом, каким это
описано в M-005: новое поведение отвечает на тот же приказ `SkillCast`, а
разводит их `CanHandle` по типу аргументов и типу действия. Вся цепочка
приказа уже полиморфна (`SkillCastCommandReceivedEventArgs.ToActionArgs()`
виртуальный), поэтому менеджеры приказов и поведений не трогаются вообще.

**Данные и события (от ответа на вопрос не зависят)**

- `Abstract/CastToTargetAction.cs` — абстрактное действие с целью-юнитом,
  пара к `CastToPointAction`. Интерфейс `ITargetSelected` уже есть.
- `Events/SkillCastToTargetCommandReceivedHandler.cs` и
  `Events/SkillCastToTargetActionStartedHandler.cs` — по образцу
  точечных: наследники с полем `Target` и переопределённым `ToActionArgs`.
- `Execution/CastToTargetActionExecutor.cs` — пара к
  `CastToPointActionExecutor`, отдаёт наследнику `Act(owner, target)`.
- `SkillActions/ApplyImpactsToTargetAction.cs` +
  `Execution/ApplyImpactsToTargetExecutor.cs` — первый конкретный вид:
  мгновенно применяет свои импакты к цели. Без снаряда: задача про тип
  каста, а не про новый снаряд. Снаряд в цель понадобится T-026, это её
  вид действия.
- `SkillActionType.ApplyImpactsToTarget` + строка в
  `SkillActionExecutorFactory`.

**Поведение**

- `UnitBehaviour/SkillCastingBehaviourBase.cs` — общее у двух кастов:
  таймер каста, проверка `CheckIfCanCast`, `StopAction`, сброс в
  `PreUpdate`. Различие ровно в двух местах: как считается дистанция до
  цели и куда идти. `SkillCastingToPointBehaviour` переезжает на базу без
  изменения поведения; выигрыш — T-004 (каст завершается чужим событием)
  становится правкой в одном месте, а не в двух.
- `UnitBehaviour/SkillCastingToTargetBehaviour.cs` — подходит к цели
  `GoToObject(target, CastRange)`, кастует, шлёт `SkillParams` с `Target`.
  Цель умерла по дороге — каст отменяется.
- `UnitBehaviourType.SkillCastingToTarget = 12` + строка в
  `UnitBehaviourFactory`.

**Ввод и фильтр цели**

- `UnitSkills.CreateCommandArgs` — ветка для `CastToTargetAction`: цель
  берётся из `_cursorMovedEventArgs.UnitUnderCursor` (поле уже есть, ввод
  не трогается). Цель не проходит `SkillTargetFilter` — команда не
  создаётся вовсе, метод возвращает null, `SkillController` молча ничего
  не делает. Так «клик по союзнику при `TargetType = Enemies`» не
  запускает каст и не тратит ману.

**Ассеты и префаб (в конце, после проверки компиляции)**

- `SkillsSection/Actions/<новое>.asset` + `SkillsSection/Skills/<новое>.asset`
  — способность с целью, импакт мгновенного урона.
- `Prefabs/GameObjects/Units/Caster Unit.prefab` — в `Behaviours`
  добавляется `SkillCastingToTarget`, в `UnitSkills.Skills` — слот на
  свободную клавишу `R` (`Q`, `W`, `E` заняты, `S` зарезервирована).

**Что НЕ делается**

- T-004 не чинится: `StopAction` в базе продолжает шлёть
  `OnMoveActionEnded`, как сейчас у точечного каста. Это её задача, и
  после переезда на базу она правится в одном месте.
- Снаряд, летящий в цель, — не здесь (T-026).
- Подсказки прицеливания — не здесь (T-024, M-020).

## Ход работы

- 2026-09-07 сделаны данные и события каста в цель:
  `Abstract/CastToTargetAction.cs`,
  `Events/SkillCastToTargetCommandReceivedHandler.cs`,
  `Events/SkillCastToTargetActionStartedHandler.cs`,
  `Execution/CastToTargetActionExecutor.cs`,
  `SkillActions/ApplyImpactsToTargetAction.cs`,
  `Execution/ApplyImpactsToTargetExecutor.cs`, значение
  `SkillActionType.ApplyImpactsToTarget` и строка в
  `SkillActionExecutorFactory`.
- 2026-09-07 поведения: заведена общая база
  `UnitBehaviour/SkillCastingBehaviourBase.cs` (таймер каста, проверка
  возможности каста, `StopAction`, сброс в `PreUpdate`; различие — четыре
  абстрактных метода про прицел). `SkillCastingToPointBehaviour` переписан
  на неё, логика та же; заодно убрано мёртвое поле `isMoving`, которое
  писалось и нигде не читалось. Новый
  `UnitBehaviour/SkillCastingToTargetBehaviour.cs` подходит к цели через
  `GoToObject(target, CastRange)`, дистанцию считает тем же
  `GetDistanceTo`, что ближний бой, и отменяет каст, если цель исчезла.
  `UnitBehaviourType.SkillCastingToTarget = 12` + строка в фабрике.
- 2026-09-07 выбор цели: `UnitSkills.CreateCommandArgs` берёт цель из
  `_cursorMovedEventArgs.UnitUnderCursor` (поле уже было, ввод не тронут).
  Цель пустая или не проходит `SkillTargetFilter` — метод возвращает null,
  `SkillController` не создаёт приказ вовсе. Он же теперь всегда сбрасывает
  `_preparedSkill` на отпускании клавиши, независимо от исхода.
- 2026-09-07 работа приостанавливалась: пользователь был в Play mode.
  `mcpforunity://editor/state` показывает `is_playing: true`. Значит
  скрипты ещё НЕ скомпилированы: в проекте включён Recompile After
  Finished Playing, Unity откладывает компиляцию до выхода из игры.
  Пустая консоль сейчас ничего не доказывает, и `SkillCastingToTargetBehaviour`
  в живой сборке пока не существует. Префаб `Caster Unit` и ассеты не
  трогаю: правка ассетов в Play mode — тот самый тихий способ потерять
  настройки. Продолжено после выхода из Play.
- 2026-09-07 после выхода из Play компиляция прошла: консоль **0 ошибок,
  0 предупреждений**, в живой сборке
  `SkillCastingToTargetBehaviour : SkillCastingBehaviourBase, IUnitBehaviour`.
- 2026-09-07 ассеты и префаб, значения прочитаны с диска после правки:
  `SkillsSection/Actions/Shock.asset` — `ApplyImpactsToTargetAction`,
  `_targetType: 0` (Enemies), импакт `InstantDamageImpact` 80 урона типа
  Magic; `SkillsSection/Skills/Shock.asset` — `ActiveSkill`, кулдаун 4,
  каст 0.3, дальность 6, мана 40, `Action` -> ассет действия;
  `Prefabs/GameObjects/Units/Caster Unit.prefab` — `Behaviours` стал
  `0,1,2,3,5,6,11,12` (добавлен `SkillCastingToTarget`, семь прежних на
  месте), в `UnitSkills.Skills` четвёртый слот: Shock на `Keycode: 114`
  (`R`, не зарезервирована). Прежние Q/W/E не тронуты.
- 2026-09-07 импакт в `Shock.asset` вписан правкой текста ассета, как в
  T-025: штатный API `[SerializeReference]`-элемент не создаёт. Unity
  подтвердил, что элемент живой — `Impacts.Array.data[0].damage`
  опознаётся как Float, `type` как Enum.
- 2026-09-07 дальность каста намеренно короткая (6 против 12.5 у `Q`),
  чтобы на плейтесте было видно, как кастер подходит к цели. Числа —
  заглушки, баланс за пользователем.

## Решения

- 2026-09-05 найдено при `/adopt`.
- 2026-09-05 (ответ в чате) это задел на будущее: направленные скиллы и
  скиллы, которые кастуются в юнита, планируются, но не сейчас. Остаётся
  в бэклоге, блокировка снята.

- 2026-09-07 расхождение: в `Решения` от 2026-09-05 записано «остаётся в
  бэклоге, не сейчас», но задача стоит в v0.1.1 и от неё зависит T-026
  («T-002 до T-026» в файле версии). Считаю версию главнее старой записи и
  делаю задачу; запись устарела на момент планирования.
- 2026-09-07 общая база двух кастовых поведений вынесена без спроса: это
  не изменение принятой механики, поведение точечного каста побайтово то
  же, а T-004 после этого правится в одном месте. Решение по коду — моё.

- 2026-09-07 (ответ в чате) выбор цели — тем же зажатием и отпусканием
  клавиши, что у точечного каста, без отдельного клика мышью. Отпустил не
  над подходящей целью — каст ОТМЕНЯЕТСЯ: приказ не создаётся, в очередь
  не встаёт, мана не тратится, кулдаун не ставится.

- 2026-09-07 первый вид действия с целью — `ApplyImpactsToTarget`
  (мгновенно применяет импакты к цели), без снаряда. Задача про тип каста;
  снаряд, летящий в юнита, — отдельный вид действия и работа T-026.
- 2026-09-07 дистанция до цели считается `GetDistanceTo` (между
  поверхностями), как в ближнем бою и в `FollowingBehaviour`, а не между
  центрами: иначе `CastRange` у крупных юнитов значил бы разное.

- 2026-09-07 (ответ в чате) приёмка пройдена: проверено, «отлично».
  Числа Shock оставлены как поставлены, способность оставлена на Caster Unit.

## Итог

В игре появился второй тип каста — в юнита. У кастера на `R` стоит Shock:
навёл на врага, отпустил клавишу — кастер подходит на 6 и бьёт на 80 Magic,
тратит 40 маны, уходит в кулдаун на 4 с. Отпустил над своим или над пустым
местом — каст отменяется молча, без траты.

Заглушек не осталось, handoff не потребовался. Числа Shock — заглушки под
плейтест, баланс за пользователем.

Тронуто общее: `SkillCastingToPointBehaviour` переехал на общую базу
(логика та же), `UnitSkills.CreateCommandArgs` теперь может вернуть null,
`SkillController` сбрасывает прицеленный скилл на отпускании клавиши,
`Caster Unit.prefab` получил поведение и слот.

T-004 (каст завершается чужим событием `OnMoveActionEnded`) сознательно не
чинилась и после переезда на базу правится в одном месте вместо двух.
