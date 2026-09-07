using System;

/// <summary>
/// Whoever this catches is stunned for duration seconds (M-019). Nothing else:
/// the damage of a stunning bolt is a separate InstantDamage impact in the same
/// action, so the two numbers are balanced apart from each other.
/// </summary>
[Serializable]
public class StunImpact : SkillUnitImpact
{
    public float duration;

    public override SkillImpactType Type => SkillImpactType.Stun;
}
