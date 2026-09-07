using UnityEngine;

public class ApplyImpactsToTargetExecutor : CastToTargetActionExecutor<ApplyImpactsToTargetAction>
{
    public ApplyImpactsToTargetExecutor(ApplyImpactsToTargetAction data) : base(data) { }

    public override void Act(GameObject owner, GameObject target)
    {
        // Checked again at the moment it lands, not only when the order was given:
        // the target may have changed sides or become invulnerable while the
        // caster was walking up to it.
        if (!SkillTargetFilter.CanHit(target, owner, Data.TargetType))
        {
            return;
        }

        SkillImpactExecutorFactory.ApplyUnitImpacts(Data, target, owner);
    }
}
