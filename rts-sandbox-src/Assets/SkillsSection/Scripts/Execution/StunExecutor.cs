using UnityEngine;

public class StunExecutor : SkillImpactExecutor<StunImpact>
{
    public StunExecutor(StunImpact data) : base(data) { }

    public override void ApplyToUnit(GameObject target, GameObject skillOwner)
    {
        // The key is the impact data itself, exactly as the poison does it: the
        // impact lives inside one action asset and an action asset belongs to one
        // skill, so two casters of the same skill refresh a single stun while a
        // stun from another skill hangs beside it (M-019).
        var stun = new StunEffect(
            key: Data,
            source: skillOwner,
            duration: Data.duration);

        UnitEffects.GetOrAdd(target)?.Apply(stun);
    }
}
