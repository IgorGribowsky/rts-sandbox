using System;

/// <summary>
/// Whoever this catches hits harder and faster for duration seconds (M-019,
/// T-053). Percent on top of the unit's own damage and attack speed: +50 speed
/// means half again as many blows in the same time.
/// </summary>
[Serializable]
public class StatBoostImpact : SkillUnitImpact, IEffectImpact
{
    public float damageBonusPercent = 50f;
    public float attackSpeedBonusPercent = 50f;
    public float duration = 10f;

    [UnityEngine.Tooltip("How the effect looks in the HUD (T-073). Empty: as the skill that put it on.")]
    public EffectInfo effectInfo;

    public EffectInfo EffectInfo => effectInfo;

    public override SkillImpactType Type => SkillImpactType.StatBoost;
}
