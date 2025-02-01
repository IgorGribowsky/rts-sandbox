using Assets.Scripts.Infrastructure.Enums;
using System;
using System.Collections.Generic;
using UnityEngine;

public class UnitValues : MonoBehaviour
{
    public int Id = 0;

    public float CurrentHp = 100;

    public float MaximumHp = 100;

    public float MovementSpeed = 5f;

    public int Rang = 100;

    public float ProducingTime = 1;

    public float Damage = 10f;

    public float AutoAttackDistance = 8f;

    public float AttackRate = 1f;

    //percent of AttackRate
    public float AttackDurationPercent = 0.5f;

    public DamageType DamageType = DamageType.Normal;

    public float AttackBreakDistance = 2f;

    public float MeleeAttackDistance = 2f;

    public bool HasRangeAttack = false;

    public float RangeAttackDistance = 10f;

    public float ProjectileSpeed = 12f;

    public GameObject RangeAttackProjectile = null;

    public bool CanProduceUnits = false;

    public bool IsBuilding = false;

    public List<GameObject> UnitsToProduce = new List<GameObject>();

    public bool IsBuilder = false;

    public List<BuildingToProduce> BuildingsToProduce = new List<BuildingToProduce>();
}

[Serializable]
public class BuildingToProduce
{
    public KeyCode KeyCode;

    public GameObject Building;
}