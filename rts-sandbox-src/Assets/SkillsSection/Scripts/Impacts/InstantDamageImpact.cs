
using Assets.Scripts.Infrastructure.Enums;
using System;
using UnityEngine;

[Serializable]
public class InstantDamageImpact : SkillUnitImpact
{
    public float damage;

    public DamageType type;

    public override void ImpactToUnit(GameObject target)
    {
        target.GetComponent<UnitEventManager>().OnDamageReceived(
            attacker: SkillAction.Skill.SkillOwner,
            damageAmount: damage,
            damageType: type);
    }
}