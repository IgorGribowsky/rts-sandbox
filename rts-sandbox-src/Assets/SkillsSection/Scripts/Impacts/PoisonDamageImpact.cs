
using Assets.Scripts.Infrastructure.Enums;
using System;
using UnityEngine;

[Serializable]
public class PoisonDamageImpact : SkillUnitImpact
{
    public float dps;

    public DamageType type;

    public float duration;

    public override void ImpactToUnit(GameObject target)
    {
        throw new NotImplementedException();
    }
}