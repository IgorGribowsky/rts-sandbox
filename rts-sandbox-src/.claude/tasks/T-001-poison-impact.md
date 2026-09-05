---
id: T-001
title: PoisonDamageImpact бросает NotImplementedException
status: todo
milestone: backlog
parent:
origin: ai
needs-design: false
blocked-by: []
mechanics: [M-015]
handoff: []
checkpoint:
created: 2026-09-05
updated: 2026-09-05
---

# T-001 · PoisonDamageImpact бросает NotImplementedException

## Что нужно
Импакт яда доступен в контекстном меню любого действия способности
(`Add Poison Damage Impact`), имеет поля `dps`, `type`, `duration` и
выглядит рабочим. При срабатывании он валит каст исключением.
Нужно либо реализовать периодический урон, либо убрать из меню.

## Как проверить
Добавить импакт яда в `ThrowLightOrb.asset`, попасть орбом по врагу.
Цель должна терять `dps` HP в секунду в течение `duration`, консоль чистая.

## План

## Ход работы

## Решения

- 2026-09-05 найдено при `/adopt`, заведено задним числом.
  `SkillsSection/Scripts/Impacts/PoisonDamageImpact.cs:17`.

## Итог
