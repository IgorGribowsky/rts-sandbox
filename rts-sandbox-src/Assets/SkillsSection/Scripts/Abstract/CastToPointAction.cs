using UnityEngine;

public abstract class CastToPointAction : ActiveSkillAction
{
    public override void Act(SkillParams skillParams)
    {
        Act(skillParams.Owner, skillParams.CastPoint);
    }

    public abstract void Act(GameObject Owner, Vector3 castPoint);
}
