using UnityEngine;

/// <summary>
/// An action aimed at a unit: everything it needs is the caster and the target.
/// Pair of CastToPointActionExecutor.
/// </summary>
public abstract class CastToTargetActionExecutor<TAction> : SkillActionExecutor<TAction>
    where TAction : CastToTargetAction
{
    protected CastToTargetActionExecutor(TAction data) : base(data) { }

    public override void Act(SkillParams skillParams)
    {
        if (skillParams.Target == null)
        {
            return;
        }

        Act(skillParams.Owner, skillParams.Target);
    }

    public abstract void Act(GameObject owner, GameObject target);
}
