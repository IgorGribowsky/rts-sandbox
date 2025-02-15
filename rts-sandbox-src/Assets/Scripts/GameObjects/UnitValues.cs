using Assets.Scripts.Infrastructure.Enums;
using System;
using System.Collections.Generic;
using UnityEngine;

public class UnitValues : MonoBehaviour
{
    public int Id = 0;

    public bool IsInvulnerable = false;

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

    public List<ResourceAmount> ResourceCost = new List<ResourceAmount>();

    public bool IsBuilding = false;

    //Move to BuildingValues
    public bool CanProduceUnits = false;

    public List<GameObject> UnitsToProduce = new List<GameObject>();
    //Move to BuildingValues


    public bool IsBuilder = false;

    public List<BuildingToProduce> BuildingsToProduce = new List<BuildingToProduce>();


    public bool IsMiner = false;
}

[Serializable]
public class BuildingToProduce
{
    public KeyCode KeyCode;

    public GameObject Building;
}