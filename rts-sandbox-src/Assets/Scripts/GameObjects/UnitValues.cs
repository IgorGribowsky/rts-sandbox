using Assets.Scripts.Infrastructure.Enums;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The live numbers of one unit. Since T-027 the component holds no data of its
/// own: it carries a copy of <see cref="UnitStats"/> taken from the type asset
/// when <see cref="UnitFactory"/> built the unit, and the properties below are
/// there so the rest of the code keeps reading <c>unitValues.MaximumHp</c> as
/// before. Change a number for the whole type in the asset, not here.
///
/// What only some objects need is not here either (T-015): production sits on
/// <see cref="BuildingValues"/>, the build menu on <see cref="BuilderValues"/>,
/// mining and cutting on <see cref="HarvestingValues"/>, mana on
/// <see cref="ManaValues"/>.
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

    public float TurnSpeed { get => Stats.TurnSpeed; set => Stats.TurnSpeed = value; }

    public float Damage { get => Stats.Damage; set => Stats.Damage = value; }

    /// <summary>
    /// Percent on top of Damage and of attack speed from effects running now
    /// (M-019). Live state only, never saved: the effects put it on and take
    /// it off themselves.
    /// </summary>
    public float DamageBonusPercent { get; private set; }
    public float AttackSpeedBonusPercent { get; private set; }

    /// <summary>What a blow deals right now, boosts included. Attacks read this, not Damage.</summary>
    public float CurrentDamage => Damage * (1f + DamageBonusPercent / 100f);

    /// <summary>
    /// Seconds between blows right now: +50% attack speed is half again as
    /// many blows, so the rate is divided, not cut by half.
    /// </summary>
    public float CurrentAttackRate => AttackRate / Mathf.Max(0.1f, 1f + AttackSpeedBonusPercent / 100f);

    /// <summary>A boost went on or came off. The HUD shows the damage with it.</summary>
    public event System.Action BoostChanged;

    public void AddBoost(float damagePercent, float attackSpeedPercent)
    {
        DamageBonusPercent += damagePercent;
        AttackSpeedBonusPercent += attackSpeedPercent;
        BoostChanged?.Invoke();
    }

    public float AutoAttackDistance { get => Stats.AutoAttackDistance; set => Stats.AutoAttackDistance = value; }

    public float AttackRate { get => Stats.AttackRate; set => Stats.AttackRate = value; }

    public float AttackDurationPercent { get => Stats.AttackDurationPercent; set => Stats.AttackDurationPercent = value; }

    public DamageType DamageType { get => Stats.DamageType; set => Stats.DamageType = value; }

    public float AttackBreakDistance { get => Stats.AttackBreakDistance; set => Stats.AttackBreakDistance = value; }

    public float MeleeAttackDistance { get => Stats.MeleeAttackDistance; set => Stats.MeleeAttackDistance = value; }

    public bool HasRangeAttack { get => Stats.HasRangeAttack; set => Stats.HasRangeAttack = value; }

    public float RangeAttackDistance { get => Stats.RangeAttackDistance; set => Stats.RangeAttackDistance = value; }

    public float SightRange { get => Stats.SightRange; set => Stats.SightRange = value; }

    public float ProjectileSpeed { get => Stats.ProjectileSpeed; set => Stats.ProjectileSpeed = value; }

    public GameObject RangeAttackProjectile { get => Stats.RangeAttackProjectile; set => Stats.RangeAttackProjectile = value; }

    public List<ResourceAmount> ResourceCost { get => Stats.ResourceCost; set => Stats.ResourceCost = value; }

    public List<ResourceAmount> SupplyResourceProduces { get => Stats.SupplyResourceProduces; set => Stats.SupplyResourceProduces = value; }

    public bool IsBuilding { get => Stats.IsBuilding; set => Stats.IsBuilding = value; }







    public bool CanCastSkills { get => Stats.CanCastSkills; set => Stats.CanCastSkills = value; }







}
