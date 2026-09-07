/// <summary>
/// An action aimed at a point on the ground: everything it needs is the caster
/// and the point.
/// </summary>
public abstract class CastToPointActionExecutor<TAction> : SkillActionExecutor<TAction>
    where TAction : CastToPointAction
{
    protected CastToPointActionExecutor(TAction data) : base(data) { }

    public override void Act(SkillParams skillParams)
    {
        Act(skillParams.Owner, skillParams.CastPoint);
    }

    public abstract void Act(UnityEngine.GameObject owner, UnityEngine.Vector3 castPoint);
}
