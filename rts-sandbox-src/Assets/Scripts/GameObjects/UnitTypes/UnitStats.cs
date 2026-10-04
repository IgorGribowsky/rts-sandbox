using Assets.Scripts.Infrastructure.Enums;
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// What every unit and building has: how much it can take, how fast it moves,
/// how hard it hits, what it costs. Roles that only some objects play live in
/// their own stats next to this one — see <see cref="BuildingStats"/>,
/// <see cref="BuilderStats"/>, <see cref="HarvestingStats"/> and
/// <see cref="ManaStats"/>.
///
/// Lives in two places and only in two: inside a <see cref="UnitTypeData"/>
/// asset, where it is the template of a type, and inside a
/// <see cref="UnitValues"/> component, where it is the live state of one unit.
/// The component gets its own copy through <see cref="Clone"/> — sharing it
/// would make every warrior lose HP together.
/// </summary>
[Serializable]
public class UnitStats
{
    [Header("Identity")]
    public int Id = 0;

    [Header("Health")]
    public bool IsInvulnerable = false;
    public float CurrentHp = 100;
    public float MaximumHp = 100;
    public float BaseHpRegen = 1;

    [Header("Movement and production")]
    public float MovementSpeed = 5f;
    public int Rang = 100;

    [Tooltip("How long this one takes to be produced or built.")]
    public float ProducingTime = 1;

    [Tooltip("Degrees per second. Before a blow or a cast the unit turns to face the aim at this speed and does not start until it does (T-033). Walking turns are the NavMeshAgent's own.")]
    public float TurnSpeed = 540f;

    [Header("Attack")]
    public float Damage = 10f;
    public float AutoAttackDistance = 8f;
    public float AttackRate = 1f;

    [Tooltip("Percent of AttackRate")]
    public float AttackDurationPercent = 0.4f;

    public DamageType DamageType = DamageType.Normal;
    public float AttackBreakDistance = 2f;
    public float MeleeAttackDistance = 2f;
    public bool HasRangeAttack = false;
    public float RangeAttackDistance = 10f;
    public float ProjectileSpeed = 12f;
    public GameObject RangeAttackProjectile = null;

    [Header("Cost")]
    public List<ResourceAmount> ResourceCost = new List<ResourceAmount>();
    public List<ResourceAmount> SupplyResourceProduces = new List<ResourceAmount>();

    [Header("Roles")]
    public bool IsBuilding = false;
    public bool CanCastSkills = false;

    /// <summary>
    /// A copy for one unit to live in. Lists get their own containers: two units
    /// of the same type must not share the list they were built from, otherwise
    /// a change on one edits the asset on disk.
    /// </summary>
    public UnitStats Clone()
    {
        var copy = (UnitStats)MemberwiseClone();
        copy.ResourceCost = new List<ResourceAmount>(ResourceCost);
        copy.SupplyResourceProduces = new List<ResourceAmount>(SupplyResourceProduces);
        return copy;
    }
}

/// <summary>
/// What a building is: how much room it takes and what it can produce. Lives on
/// <see cref="BuildingValues"/>. A warrior has none of this, which is why it
/// left <see cref="UnitStats"/> (T-015).
/// </summary>
[Serializable]
public class BuildingStats
{
    [Tooltip("Cells the building takes on the build grid.")]
    public int GridSize = 4;

    [Tooltip("Size of the NavMesh obstacle under the building.")]
    public int ObstacleSize = 4;

    [Tooltip("A resource on the map rather than something the player put down.")]
    public bool IsResource = false;

    public bool CanProduceUnits = false;

    public List<UnitTypeData> UnitsToProduce = new List<UnitTypeData>();

    public BuildingStats Clone()
    {
        var copy = (BuildingStats)MemberwiseClone();
        copy.UnitsToProduce = new List<UnitTypeData>(UnitsToProduce);
        return copy;
    }
}

/// <summary>What a builder can put down. Lives on <see cref="BuilderValues"/>.</summary>
[Serializable]
public class BuilderStats
{
    public bool IsBuilder = false;

    public List<BuildingToProduce> BuildingsToProduce = new List<BuildingToProduce>();

    public BuilderStats Clone()
    {
        var copy = (BuilderStats)MemberwiseClone();
        copy.BuildingsToProduce = new List<BuildingToProduce>(BuildingsToProduce);
        return copy;
    }
}

/// <summary>
/// Mining and woodcutting. Lives on <see cref="HarvestingValues"/>: a wall has
/// no use for a harvesting rate.
/// </summary>
[Serializable]
public class HarvestingStats
{
    public bool IsMiner = false;
    public bool IsHarvestor = false;

    public List<ResourceName> ResourcesCanBeHarvested = new List<ResourceName>();

    public float HarvestingRate = 1f;
    public int HarvestingValuePerTick = 1;
    public int HarvestingMaxValue = 10;

    public HarvestingStats Clone()
    {
        var copy = (HarvestingStats)MemberwiseClone();
        copy.ResourcesCanBeHarvested = new List<ResourceName>(ResourcesCanBeHarvested);
        return copy;
    }
}

/// <summary>The mana pool. Lives on <see cref="ManaValues"/>, next to UnitManaPoints.</summary>
[Serializable]
public class ManaStats
{
    public float CurrentMana = 0;
    public float MaximumMana = 0;
    public float BaseManaRegen = 1;

    public ManaStats Clone()
    {
        return (ManaStats)MemberwiseClone();
    }
}

/// <summary>One line of a builder's menu: which key puts down which type.</summary>
[Serializable]
public class BuildingToProduce
{
    public KeyCode KeyCode;

    public UnitTypeData Building;
}
