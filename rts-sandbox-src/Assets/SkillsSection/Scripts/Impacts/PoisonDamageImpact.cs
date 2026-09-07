using Assets.Scripts.Infrastructure.Enums;
using System;

[Serializable]
public class PoisonDamageImpact : SkillUnitImpact
{
    public float dps;

    public DamageType type;

    public float duration;

    /// <summary>How often the damage lands. 0 means GameConstants.DefaultEffectTickRate.</summary>
    public float tickInterval;

    public override SkillImpactType Type => SkillImpactType.PoisonDamage;
}
