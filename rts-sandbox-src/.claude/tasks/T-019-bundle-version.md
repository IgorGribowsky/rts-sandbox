---
id: T-019
title: bundleVersion отстал от рабочей ветки
status: todo
milestone: backlog
parent:
origin: ai
needs-design: false
blocked-by: []
mechanics: []
handoff: []
checkpoint:
created: 2026-09-05
updated: 2026-09-05
---

# T-019 · bundleVersion отстал от рабочей ветки

## Что нужно
`ProjectSettings.bundleVersion` = `0.0.5-alpha`, работа идёт в ветке
`0.1.1-alpha`. Версии 0.0.6 и 0.1.0 закрыты и отмечены тегами
(`v0.0.6-alpha`, `v0.1.0-alpha`), но в настройках проекта это не отражено.

Поставить `0.1.1-alpha` через `PlayerSettings`, не правкой текста в
`ProjectSettings.asset`.

## Как проверить
Edit > Project Settings > Player: Version показывает `0.1.1-alpha`.

## План

## Ход работы

## Решения

- 2026-09-05 найдено при `/adopt`. По правилу пайплайна версию ставят при
  взятии первой задачи новой версии; здесь версия уже идёт, а число не
  обновлено — закрываю отдельной задачей.
- 2026-09-05 (ответ в чате) «забыл поменять», то есть расхождение
  подтверждено, а не задумано. Сам менять не стал: правка `bundleVersion`
  из кода в Edit mode — ровно то, что пайплайн запрещает делать молча.
  Поставлю при взятии первой задачи 0.1.1-alpha, или сделай руками через
  Project Settings > Player.

## Итог
