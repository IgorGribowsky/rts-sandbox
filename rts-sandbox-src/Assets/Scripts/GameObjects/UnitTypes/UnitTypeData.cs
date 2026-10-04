using Assets.Scripts.GameObjects.UnitBehaviour;
using Assets.Scripts.Infrastructure.Enums;
using Assets.SkillsSection.Scripts;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A type of unit, building or resource written down as data. Everything the
/// game needs to build one is here: what it looks like, what scripts it gets,
/// and every number it starts with.
///
/// Nothing instantiates a unit prefab any more — production, building and the
/// scene itself all go through <see cref="UnitFactory"/> with one of these
/// assets. A prefab now only carries the body: mesh, collider, agent and the
/// bar canvas. Gameplay scripts are hung by the factory from the flags below.
/// </summary>
[CreateAssetMenu(fileName = "New Unit Type", menuName = "RTS/Unit Type", order = 0)]
public class UnitTypeData : ScriptableObject
{
    [Header("Identity")]
    [Tooltip("Shown in the editor and in reports. Not used to find the type.")]
    public string DisplayName;

    [Tooltip("What it is for, a sentence or two, for the card tooltip (M-024).")]
    [TextArea(2, 5)]
    public string Description;

    [Tooltip("The body: mesh, materials, collider, NavMeshAgent or obstacle, " +
             "the BarCanvas child. No gameplay scripts — the factory adds those.")]
    public GameObject BodyPrefab;

    [Header("What it is made of")]
    [Tooltip("Belongs to a team and is painted in its colour.")]
    public bool HasTeam = true;

    [Tooltip("Has numbers of its own: adds UnitValues and UnitEventManager. " +
             "A tree has neither — it is scenery with a resource in it.")]
    public bool HasValues = true;

    public bool IsSelectable = true;

    [Tooltip("Circle drawn under the unit when selected. Size differs per unit.")]
    public GameObject SelectionCirclePrefab;

    [Tooltip("Health, damage and death. Without it the object cannot be hurt at all.")]
    public bool HasHealth = true;

    [Tooltip("Calls allies for help when hit.")]
    public bool CallsAlliesWhenAttacked = true;

    [Tooltip("Moves on the NavMesh: adds NavMeshMovement. The agent itself lives on the body.")]
    public bool IsMobile = false;

    [Tooltip("Takes orders: adds UnitCommandManager and UnitBehaviourManager.")]
    public bool TakesCommands = true;

    [Tooltip("A building: adds Building and BuildingValues, is put down by a builder.")]
    public bool IsBuildingObject = false;

    [Tooltip("Eats supply on birth and frees it on death.")]
    public bool RequiresSupply = false;

    [Tooltip("Raises the supply limit, like a farm.")]
    public bool ProducesSupply = false;

    [Tooltip("Workers bring harvested resources here.")]
    public bool StoresHarvestedResources = false;

    [Tooltip("A resource on the map: adds ResourceValues.")]
    public bool IsResourceObject = false;

    [Header("Behaviours")]
    [Tooltip("What this type is able to do. Fed into UnitBehaviourManager.")]
    public List<UnitBehaviourType> Behaviours = new List<UnitBehaviourType>();

    [Header("Numbers")]
    public UnitStats Stats = new UnitStats();

    [Tooltip("Footprint and unit production. Used when IsBuildingObject or IsResourceObject.")]
    public BuildingStats Building = new BuildingStats();

    [Tooltip("What this one can put down. A component appears only when IsBuilder is on.")]
    public BuilderStats Builder = new BuilderStats();

    [Tooltip("Mining and cutting. A component appears only when IsMiner or IsHarvestor is on.")]
    public HarvestingStats Harvesting = new HarvestingStats();

    [Tooltip("Mana pool. A component appears only when HasMana is on.")]
    public ManaStats Mana = new ManaStats();

    [Header("Resource")]
    public bool IsMine = false;
    public bool IsHeldMine = false;
    public bool IsHarvestedResource = false;
    [Tooltip("What this mine or tree gives (T-052).")]
    public ResourceDefinition Resource;
    public int ResourcesAmount = 0;
    [Tooltip("Harvested resource only: how many workers cut at it at once; the rest go to the nearest one with room.")]
    public int MaxHarvesters = 3;

    [Header("Held mine")]
    [Tooltip("What is left on the map when the held mine is destroyed.")]
    public UnitTypeData ParentMineType;

    public float MiningRate = 1f;
    public int MiningValue = 10;
    public int MinersMaxCount = 5;

    [Header("Storage")]
    [Tooltip("What workers may hand in here (T-052).")]
    public List<ResourceDefinition> ResourcesToStore = new List<ResourceDefinition>();

    [Header("Bars")]
    public GameObject HealthBarTemplate;
    public int HealthBarPriority = 0;
    public GameObject ManaBarTemplate;
    public int ManaBarPriority = 0;
    public GameObject ProducingBarTemplate;
    public int ProducingBarPriority = 0;

    [Header("Skills")]
    [Tooltip("Skill on a key. Empty list means the unit gets no UnitSkills at all.")]
    public List<UnitSkills.SkillSlot> Skills = new List<UnitSkills.SkillSlot>();

    [Tooltip("Mana pool and its bar. Off for a unit with skills but no mana cost.")]
    public bool HasMana = false;

    [Header("Experience")]
    [Tooltip("Gathers experience and levels up (M-026). Needs a table.")]
    public bool HasLevels = false;

    [Tooltip("Level-up costs and the share radius. Used when HasLevels is on.")]
    public ExperienceTable ExperienceTable;

    [Tooltip("Experience shared among the enemy's units with levels nearby when this one dies.")]
    public int ExperienceReward = 0;

    /// <summary>The unit gathers, so it gets a HarvestingValues component.</summary>
    public bool HasHarvesting => Harvesting.IsMiner || Harvesting.IsHarvestor;

    /// <summary>Same number as <c>UnitValues.Id</c>: what tells one type from another.</summary>
    public int Id => Stats.Id;

    /// <summary>The unit is put together with a UnitSkills component.</summary>
    public bool HasSkills => Skills != null && Skills.Count > 0;
}
