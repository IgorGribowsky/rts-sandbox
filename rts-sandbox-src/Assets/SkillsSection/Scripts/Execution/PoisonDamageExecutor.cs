using Assets.Scripts.Infrastructure.Constants;
using UnityEngine;

public class PoisonDamageExecutor : SkillImpactExecutor<PoisonDamageImpact>
{
    public PoisonDamageExecutor(PoisonDamageImpact data) : base(data) { }

    public override void ApplyToUnit(GameObject target, GameObject skillOwner)
    {
        var tickInterval = Data.tickInterval > 0 ? Data.tickInterval : GameConstants.DefaultEffectTickRate;

        // The key is the impact data itself: it lives inside one action asset, and
        // an action asset belongs to one skill. So two casters of the same skill
        // refresh a single poison, while poison from another skill hangs in
        // parallel — the rule of M-019 without dragging the skill down here.
        var poison = new PoisonEffect(
            key: Data,
            source: skillOwner,
            dps: Data.dps,
            damageType: Data.type,
            duration: Data.duration,
            tickInterval: tickInterval);

        UnitEffects.GetOrAdd(target)?.Apply(poison);
    }
}
