
using Assets.Scripts.Infrastructure.Enums;
using System;
using UnityEngine;

[Serializable]
public class InstantDamageImpact : SkillUnitImpact
{
    public float damage;

    public DamageType type;

    public override void ImpactToUnit(GameObject target, GameObject skillOwner)
    {
        target.GetComponent<UnitEventManager>().OnDamageReceived(
            attacker: skillOwner,
            damageAmount: damage,
            damageType: type);
    }
}