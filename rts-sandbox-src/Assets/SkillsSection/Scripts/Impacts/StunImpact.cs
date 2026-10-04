using System;

/// <summary>
/// Whoever this catches is stunned for duration seconds (M-019). Nothing else:
/// the damage of a stunning bolt is a separate InstantDamage impact in the same
/// action, so the two numbers are balanced apart from each other.
/// </summary>
[Serializable]
public class StunImpact : SkillUnitImpact, IEffectImpact
{
    public float duration;

    [UnityEngine.Tooltip("How the effect looks in the HUD (T-073). Empty: as the skill that put it on.")]
    public EffectInfo effectInfo;

    public EffectInfo EffectInfo => effectInfo;

    public override SkillImpactType Type => SkillImpactType.Stun;
}
