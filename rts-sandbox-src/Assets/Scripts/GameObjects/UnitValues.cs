using Assets.Scripts.Infrastructure.Enums;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The live numbers of one unit. Since T-027 the component holds no data of its
/// own: it carries a copy of <see cref="UnitStats"/> taken from the type asset
/// when <see cref="UnitFactory"/> built the unit, and the properties below are
/// there so the rest of the code keeps reading <c>unitValues.MaximumHp</c> as
/// before. Change a number for the whole type in the asset, not here.
/// </summary>
public class UnitValues : MonoBehaviour
{
    [Tooltip("The type this unit was built from. Read-only at runtime.")]
    public UnitTypeData Type;

    [Tooltip("This unit's own copy of the type numbers. Editing it in play mode " +
             "changes this unit only.")]
    public UnitStats Stats = new UnitStats();

    public int Id { get => Stats.Id; set => Stats.Id = value; }

    public bool IsInvulnerable { get => Stats.IsInvulnerable; set => Stats.IsInvulnerable = value; }

    public float CurrentHp { get => Stats.CurrentHp; set => Stats.CurrentHp = value; }

    public float MaximumHp { get => Stats.MaximumHp; set => Stats.MaximumHp = value; }

    public float BaseHpRegen { get => Stats.BaseHpRegen; set => Stats.BaseHpRegen = value; }

    public float MovementSpeed { get => Stats.MovementSpeed; set => Stats.MovementSpeed = value; }

    public int Rang { get => Stats.Rang; set => Stats.Rang = value; }

    public float ProducingTime { get => Stats.ProducingTime; set => Stats.ProducingTime = value; }

    public float Damage { get => Stats.Damage; set => Stats.Damage = value; }

    public float AutoAttackDistance { get => Stats.AutoAttackDistance; set => Stats.AutoAttackDistance = value; }

    public float AttackRate { get => Stats.AttackRate; set => Stats.AttackRate = value; }

    public float AttackDurationPercent { get => Stats.AttackDurationPercent; set => Stats.AttackDurationPercent = value; }

    public DamageType DamageType { get => Stats.DamageType; set => Stats.DamageType = value; }

    public float AttackBreakDistance { get => Stats.AttackBreakDistance; set => Stats.AttackBreakDistance = value; }

    public float MeleeAttackDistance { get => Stats.MeleeAttackDistance; set => Stats.MeleeAttackDistance = value; }

    public bool HasRangeAttack { get => Stats.HasRangeAttack; set => Stats.HasRangeAttack = value; }

    public float RangeAttackDistance { get => Stats.RangeAttackDistance; set => Stats.RangeAttackDistance = value; }

    public float ProjectileSpeed { get => Stats.ProjectileSpeed; set => Stats.ProjectileSpeed = value; }

    public GameObject RangeAttackProjectile { get => Stats.RangeAttackProjectile; set => Stats.RangeAttackProjectile = value; }

    public List<ResourceAmount> ResourceCost { get => Stats.ResourceCost; set => Stats.ResourceCost = value; }

    public List<ResourceAmount> SupplyResourceProduces { get => Stats.SupplyResourceProduces; set => Stats.SupplyResourceProduces = value; }

    public bool IsBuilding { get => Stats.IsBuilding; set => Stats.IsBuilding = value; }

    public bool CanProduceUnits { get => Stats.CanProduceUnits; set => Stats.CanProduceUnits = value; }

    public List<UnitTypeData> UnitsToProduce { get => Stats.UnitsToProduce; set => Stats.UnitsToProduce = value; }

    public bool IsBuilder { get => Stats.IsBuilder; set => Stats.IsBuilder = value; }

    public List<BuildingToProduce> BuildingsToProduce { get => Stats.BuildingsToProduce; set => Stats.BuildingsToProduce = value; }

    public bool IsMiner { get => Stats.IsMiner; set => Stats.IsMiner = value; }

    public bool IsHarvestor { get => Stats.IsHarvestor; set => Stats.IsHarvestor = value; }

    public bool CanCastSkills { get => Stats.CanCastSkills; set => Stats.CanCastSkills = value; }

    public List<ResourceName> ResourcesCanBeHarvested { get => Stats.ResourcesCanBeHarvested; set => Stats.ResourcesCanBeHarvested = value; }

    public float HarvestingRate { get => Stats.HarvestingRate; set => Stats.HarvestingRate = value; }

    public int HarvestingValuePerTick { get => Stats.HarvestingValuePerTick; set => Stats.HarvestingValuePerTick = value; }

    public int HarvestingMaxValue { get => Stats.HarvestingMaxValue; set => Stats.HarvestingMaxValue = value; }

    public float CurrentMana { get => Stats.CurrentMana; set => Stats.CurrentMana = value; }

    public float MaximumMana { get => Stats.MaximumMana; set => Stats.MaximumMana = value; }

    public float BaseManaRegen { get => Stats.BaseManaRegen; set => Stats.BaseManaRegen = value; }
}
