using Assets.Scripts.Infrastructure.Enums;
using System;

[Serializable]
public class InstantDamageImpact : SkillUnitImpact
{
    public float damage;

    public DamageType type;

    public override SkillImpactType Type => SkillImpactType.InstantDamage;
}
