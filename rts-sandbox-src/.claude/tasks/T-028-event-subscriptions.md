---
id: T-028
title: Подписки на события привести к OnEnable/OnDisable
status: todo
milestone: v0.2.0
parent:
origin: user
needs-design: false
blocked-by: []
mechanics: [M-004]
handoff: []
checkpoint:
created: 2026-09-05
updated: 2026-09-05
---

# T-028 · Подписки на события привести к OnEnable/OnDisable

## Что нужно
Сейчас подписки разбросаны по трём разным местам, и единого правила нет:
одни компоненты подписываются в `Awake`, другие в `Start`, отписываются
почти все в `OnDestroy`, а `UnitHealthPoints` и `UnitManaPoints` уже
используют `OnEnable`/`OnDisable`, но только для корутин.

Формулировка пользователя: «а то сейчас хаос, надо привести к общему виду».
Правило — подписка в `OnEnable`, отписка в `OnDisable`.

Почему это не косметика: `Building` выключает `UnitProducing`,
`UnitCommandManager` и `HarvestedResourcesStorage` на время стройки и
включает обратно. Компонент, подписавшийся в `Awake`, продолжает получать
события выключенным.

Найденные при `/adopt` дыры, которые закрываются этой же задачей:

- `UnitCommandManager.OnDestroy` не отписывает `SkillCastCommandReceived`
  и `SkillCastActionEnded` — две из девяти пар;
- `UnitBehaviourManager` подписывает `SkillCastActionStarted` безусловно,
  а отписывает только при наличии `SkillCastingToPointBehaviour`.

Обе перечислены и в T-004. T-004 закрывается раньше, в 0.1.1, и чинит их
точечно; эта задача делает правило общим для всего проекта.

В истории есть коммит `0.1.0-alpha: Done Investigate unsubscribe necessity
on disable object and implement it #34` — тему уже разбирали, стоит
посмотреть, к какому выводу тогда пришли.

## Как проверить
Смоук-чек целиком. Отдельно: выключить и включить юнита в инспекторе в
Play mode — события продолжают работать, дублей не появляется. Убить
юнита во время стройки, каста и добычи — консоль чистая.

## План

## Ход работы

## Решения

- 2026-09-05 (концепт).

## Итог
