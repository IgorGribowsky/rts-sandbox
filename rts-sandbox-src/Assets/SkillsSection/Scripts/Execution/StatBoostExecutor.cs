using UnityEngine;

public class StatBoostExecutor : SkillImpactExecutor<StatBoostImpact>
{
    public StatBoostExecutor(StatBoostImpact data) : base(data) { }

    public override void ApplyToUnit(GameObject target, GameObject skillOwner)
    {
        // Keyed by the impact data, as the stun and the poison are: the same
        // skill cast again refreshes the boost instead of stacking it (M-019).
        var boost = new StatBoostEffect(
            key: Data,
            source: skillOwner,
            duration: Data.duration,
            damagePercent: Data.damageBonusPercent,
            attackSpeedPercent: Data.attackSpeedBonusPercent);

        UnitEffects.GetOrAdd(target)?.Apply(boost);
    }
}
