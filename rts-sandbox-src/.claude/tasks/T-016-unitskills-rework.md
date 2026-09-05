---
id: T-016
title: UnitSkills требует переработки
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

# T-016 · UnitSkills требует переработки

## Что нужно
В файле `UnitSkills.cs` стоит пометка автора: «There are a lot of
architecture problems in this file. So it should be reworked in future».
Что конкретно видно из кода:

- `SetupNavigationProperties` пишет `Skill` и `SkillAction` **в сам
  ScriptableObject** при старте каждого юнита. Ассет общий на всех, значит
  каждый новый юнит переписывает эти ссылки, а в редакторе они остаются
  записанными после выхода из Play mode;
- кулдаун и текущая мана живут в `UnitSkill` внутри списка на компоненте,
  а `Skills` — публичное сериализуемое поле, то есть состояние боя лежит
  вперемешку с настройкой;
- список запрещённых клавиш `banned` продублирован здесь и в
  `WindowsInputController.usedKeys`, и списки не совпадают;
- `UnitsController` держит логику выбора кастера (`TryGetSkillCaster`,
  `prepearedSkill`) с собственной пометкой автора, что это стоит вынести
  в отдельный `SkillController`.

Задача крупная и наверняка разобьётся. Первым шагом — решить, что из
перечисленного реально мешает, а что живёт нормально.

## Как проверить
Кастер кастует все три способности, кулдауны и мана считаются верно.
После выхода из Play mode ассеты в `SkillsSection/` не изменены
(проверить `git status`).

## План

## Ход работы

## Решения

- 2026-09-05 найдено при `/adopt`. Пометка про запись в ScriptableObject —
  находка при чтении, автором не отмечена.

## Итог
