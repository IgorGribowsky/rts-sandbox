---
id: H-001
title: Модели для Builder, Barracks и Wall вместо примитивных кубов
status: ready
blocking: no
kind: модель
target: Assets/Models/
task:
created: 2026-09-05
---

## Нужно
Три модели вместо стандартного куба Unity:

- **Builder** — рабочий, по размеру примерно как Warrior;
- **Barracks** — казарма, занимает 5x5 клеток сетки, препятствие 3x3;
- **Wall** — секция стены, 2x2 клетки.

Формат — `.fbx`, как остальные модели проекта. Масштаб такой же, как у
существующих: юнит примерно в одну клетку сетки, здание — в свой
`ObstacleSize`. Цвет неважен: `TeamMember` перекрашивает материал в цвет
команды при старте.

## Сейчас стоит
Стандартный куб Unity (mesh с fileID 10202). Остальные объекты уже на
своих моделях: `WarriorCube.fbx` (Warrior и Giant), `MageCube.fbx`
(Caster), `figure.fbx` (Range Unit), `Castle.fbx`, `farm.fbx`, `mine.fbx`,
`mine_held.fbx`, `cube2.fbx` (Tower).

## Когда появится
Импортирую в `Assets/Models/`, подменю Mesh Filter на префабах
`Builder.prefab`, `Barracks.prefab`, `Wall.prefab`, подгоню масштаб под
`ObstacleSize` и проверю, что коллайдер и `NavMeshObstacle` остались
прежними, а круг выделения по-прежнему помещается под юнитом.

## Стиль
Проект уже в стиле низкополигональных примитивов с плоскими материалами —
достаточно попасть в него. `references/` по этому проекту пуст.
