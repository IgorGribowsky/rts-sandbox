using Assets.Scripts.Infrastructure.Enums;
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Every number and flag a unit or a building is made of. Lives in two places
/// and only in two: inside a <see cref="UnitTypeData"/> asset, where it is the
/// template of a type, and inside a <see cref="UnitValues"/> component, where
/// it is the live state of one unit. The component gets its own copy through
/// <see cref="Clone"/> — sharing it would make every warrior lose HP together.
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
    public float ProducingTime = 1;

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
    public bool CanProduceUnits = false;
    public bool IsBuilder = false;
    public bool IsMiner = false;
    public bool IsHarvestor = false;
    public bool CanCastSkills = false;

    [Header("Production")]
    public List<UnitTypeData> UnitsToProduce = new List<UnitTypeData>();
    public List<BuildingToProduce> BuildingsToProduce = new List<BuildingToProduce>();

    [Header("Harvesting")]
    public List<ResourceName> ResourcesCanBeHarvested = new List<ResourceName>();
    public float HarvestingRate = 1f;
    public int HarvestingValuePerTick = 1;
    public int HarvestingMaxValue = 10;

    [Header("Mana")]
    public float CurrentMana = 0;
    public float MaximumMana = 0;
    public float BaseManaRegen = 1;

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
        copy.UnitsToProduce = new List<UnitTypeData>(UnitsToProduce);
        copy.BuildingsToProduce = new List<BuildingToProduce>(BuildingsToProduce);
        copy.ResourcesCanBeHarvested = new List<ResourceName>(ResourcesCanBeHarvested);
        return copy;
    }
}

/// <summary>One line of a builder's menu: which key puts down which type.</summary>
[Serializable]
public class BuildingToProduce
{
    public KeyCode KeyCode;

    public UnitTypeData Building;
}
