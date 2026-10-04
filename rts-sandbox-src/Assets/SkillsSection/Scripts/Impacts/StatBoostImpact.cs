using System;

/// <summary>
/// Whoever this catches hits harder and faster for duration seconds (M-019,
/// T-053). Percent on top of the unit's own damage and attack speed: +50 speed
/// means half again as many blows in the same time.
/// </summary>
[Serializable]
public class StatBoostImpact : SkillUnitImpact
{
    public float damageBonusPercent = 50f;
    public float attackSpeedBonusPercent = 50f;
    public float duration = 10f;

    public override SkillImpactType Type => SkillImpactType.StatBoost;
}
