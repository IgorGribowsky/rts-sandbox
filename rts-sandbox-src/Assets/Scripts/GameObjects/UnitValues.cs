using Assets.Scripts.Infrastructure.Enums;
using UnityEngine;

public class UnitValues : MonoBehaviour
{
    public float CurrentHp = 100;

    public float MaximumHp = 100;

    public float MovementSpeed = 5f;

    public int Rang = 100;

    public float Damage = 10f;

    public float AttackRate = 1f;

    public DamageType DamageType = DamageType.Normal;

    public float AttackBreakDistance = 2f;

    //percent of AttackRate
    public float AttackDurationPercent = 0.5f;

    public float MeleeAttackDistance = 2f;

    public bool HasRangeAttack = false;

    public float RangeAttackDistance = 10f;

    public GameObject RangeAttackProjectile = null;

    public float ProjectileSpeed = 12f;
}
