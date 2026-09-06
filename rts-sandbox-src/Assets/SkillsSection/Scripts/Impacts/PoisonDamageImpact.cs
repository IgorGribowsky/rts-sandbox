using Assets.Scripts.Infrastructure.Enums;
using System;

[Serializable]
public class PoisonDamageImpact : SkillUnitImpact
{
    public float dps;

    public DamageType type;

    public float duration;

    public override SkillImpactType Type => SkillImpactType.PoisonDamage;
}
