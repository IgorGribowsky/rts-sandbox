# Карта кода

Генерируемый файл, пересобирается `/map`. Правки руками затрутся.
Что делают системы — в `docs/mechanics/`. Где они лежат — здесь.

Собрано 2026-09-05 из кода на ветке `0.1.1-alpha`.

## Точка входа

Стартовая сцена одна: `Assets/Scenes/SampleScene.unity` (единственная в
build settings). `Assets/Scenes/TestScene.unity` — вспомогательная, в билд
не входит. `Assets/_Recovery/0.unity` — мусор от восстановления проекта.

Порядок подъёма — через теги, а не через явный bootstrap:

```
GameController  (tag GameController)  TeamController · GameResources · GameController · MapValues
PlayerController(tag PlayerController) PlayerTeamMember · UnitsController · BuildingController
                                       BuildingGridController · PlayerResources · PlayerEventController
                                       CameraController · SelectionBoxController
InputController                        WindowsInputController -> Controller = PlayerController
SceneController                        TestScenarios · FPSTracker
```

`GameServices` (статика) держит `TeamController` и `PlayerEventController`,
их проставляют `Awake()` соответствующих компонентов. Остальные находят друг
друга через `GameObject.FindGameObjectWithTag` в `Awake`/`Start`.

## Системы

| Система | Файлы | За что отвечает | Дока |
|---|---|---|---|
| Ввод | `Scripts/WindowsInputController.cs` | вся мышь и клавиатура, legacy Input Manager | M-001 |
| Камера | `Scripts/CameraController.cs` | скролл у краёв, зум, границы карты | M-002 |
| Выделение | `Scripts/UnitsController.cs`, `SelectionBoxController.cs`, `GameObjects/Selectable.cs` | рамка, двойной клик, кто «главный» в группе | M-003 |
| Приказы | `GameObjects/UnitCommandManager.cs`, `Infrastructure/Abstractions/ICommand.cs` | очередь команд юнита, Shift-очередь | M-004 |
| События юнита | `GameObjects/UnitEventManager.cs`, `Infrastructure/Events/*` | шина событий одного юнита | M-004 |
| События игрока | `Scripts/PlayerEventController.cs` | глобальная шина: курсор, ресурсы, стройка | M-004 |
| Поведения | `GameObjects/UnitBehaviour/*` | исполнители приказов, ровно одно активно | M-005 |
| Движение | `GameObjects/NavMeshMovement.cs` | обёртка NavMeshAgent, подход к объекту | M-006 |
| Бой | `UnitBehaviour/{Melee,Range}AttackingBehaviour.cs`, `AutoAttack*`, `GameObjects/Projectiles/ProjectileBehavior.cs` | атака, автоатака, снаряды | M-007 |
| Здоровье | `GameObjects/UnitHealthPoints.cs`, `CallingToAttackWhenAttacked.cs` | урон, смерть, реген, зов на помощь | M-008 |
| Команды/альянсы | `Scripts/TeamController.cs`, `GameObjects/TeamMember.cs`, `Scripts/PlayerTeamMember.cs` | кто кому свой | M-009 |
| Строительство | `Scripts/BuildingController.cs`, `BuildingGridController.cs`, `GameObjects/Building.cs`, `BuildingValues.cs`, `GridSegment.cs` | режим стройки, сетка, прогресс | M-010 |
| Производство | `GameObjects/UnitProducing.cs` | очередь юнитов в здании | M-011 |
| Ресурсы | `Resources/{PlayerResources,GameResources,ResourceValues,UnitSupply*}.cs` | золото/дерево/еда, лимит снабжения | M-012 |
| Шахта | `Resources/HeldMine.cs`, `UnitBehaviour/MiningBehaviour.cs` | добыча золота из захваченной шахты | M-013 |
| Сбор дерева | `UnitBehaviour/HarvestingBehaviour.cs`, `Resources/{HarvestedResource,HarvestedResourcesStorage}.cs` | рубка и сдача на склад | M-014 |
| Способности | `SkillsSection/Scripts/**` | скиллы, импакты, каст в точку | M-015 |
| Мана | `SkillsSection/Scripts/UnitManaPoints.cs` | мана и её реген | M-016 |
| Полоски | `GameObjects/{BarBase,BarsContaining,HealthPointsBar,ProducingBar}.cs`, `SkillsSection/Scripts/ManaPointsBar.cs` | HP/мана/производство над юнитом | M-017 |
| Каталог юнитов | `GameObjects/UnitValues.cs` + префабы | все числа юнитов и зданий | M-018 |
| Отладка | `Scripts/Test/TestScenarios.cs`, `Scripts/FPSTracker.cs` | горячие клавиши стравливания армий, лог FPS | M-018 |

## Инфраструктура

- `Infrastructure/Enums/` — `Tag`, `Layer` (MovementSurface 6, Unit 7,
  HarvestedResource 8), `DamageType`, `ResourceName`, `ResourceType`.
- `Infrastructure/Constants/GameConstants.cs` — все пороги дистанций и
  таймингов, которых нет в префабах.
- `Infrastructure/Events/` — 40 делегатов + `*EventArgs`, по одному файлу.
- `Infrastructure/Extensions/GameObjectExtensions.cs` — **namespace
  `Assets.Scripts.Infrastructure.Helpers`**, не `Extensions`. Здесь
  `GetDistanceTo`, `GetSize`, поиск целей в радиусе.

## Сцены и ключевые префабы

- `Assets/Prefabs/GameObjects/Units/` — Warrior, Range Unit, Giant Unit,
  Caster Unit, Builder.
- `.../Units/Buildings/` — Castle, Barracks, Farm, Tower, Wall, mine_held.
- `.../Resources/` — Tree, mine.
- `.../Bars/` — HPBar, ManaBar, ProcBar (шаблоны полосок).
- `.../Projectiles/Bullet.prefab` — снаряд обычной дальней атаки.
- `Assets/SkillsSection/Projectiles/` — LightOrbProjectile, WaveProjectile.
- `Assets/Prefabs/{Selection,Medium…,SemiBig…,Big…}SelectionCircle.prefab`,
  `BuildingCell.prefab`, `BuildingGrid.prefab`.

## Конфиги

ScriptableObject-ов баланса нет — все числа лежат в компонентах префабов
(`UnitValues`, `BuildingValues`, `ResourceValues`, `HeldMine`) и в
`GameConstants`. Единственные SO — способности:

- `Assets/SkillsSection/Skills/{LightOrb,WaterWave,Blink}.asset` — `ActiveSkill`
- `Assets/SkillsSection/Actions/{ThrowLightOrb,ThrowWaterWave,Blink}.asset` —
  действия; импакты внутри через `[SerializeReference]`.

Читает их `UnitSkills` на префабе юнита (список `Skills` + горячая клавиша).

## Чужое

- `Assets/Darth_Artisan/Free_Trees/` — бесплатный ассет-пак деревьев.
- `com.coplaydev.unity-mcp` — MCP-мост, не игровой код.

## Заглушки

- `SkillsSection/Scripts/Impacts/PoisonDamageImpact.cs:17` —
  `throw new NotImplementedException()`.
- `UnitBehaviour/UnitBehaviourManager.cs:115` — закомментирован
  `SkillCastingToTargetBehaviour`, класса нет.
- `Abstract/TargetedSkillAction.cs` — только интерфейс `ITargetSelected`,
  реализации каста в цель нет.
- `PassiveSkill` / `PassiveSkillAction` — типы есть, исполнителя нет.
- Примитивные кубы вместо моделей: Builder, Barracks, Wall (см. H-001).
