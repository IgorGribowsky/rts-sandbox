using System;
using UnityEngine;

public class PoisonDamageExecutor : SkillImpactExecutor<PoisonDamageImpact>
{
    public PoisonDamageExecutor(PoisonDamageImpact data) : base(data) { }

    // TODO(T-001): damage over time, no executor for it yet.
    public override void ApplyToUnit(GameObject target, GameObject skillOwner)
    {
        throw new NotImplementedException();
    }
}
