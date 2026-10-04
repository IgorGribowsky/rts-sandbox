using Assets.Scripts.Infrastructure.Enums;
using System;

[Serializable]
public class PoisonDamageImpact : SkillUnitImpact, IEffectImpact
{
    public float dps;

    [UnityEngine.Tooltip("How the effect looks in the HUD (T-073). Empty: as the skill that put it on.")]
    public EffectInfo effectInfo;

    public EffectInfo EffectInfo => effectInfo;

    public DamageType type;

    public float duration;

    /// <summary>How often the damage lands. 0 means GameConstants.DefaultEffectTickRate.</summary>
    public float tickInterval;

    public override SkillImpactType Type => SkillImpactType.PoisonDamage;
}
