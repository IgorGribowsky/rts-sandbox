using Assets.SkillsSection.Scripts.Events;
using System;
using UnityEngine;

/// <summary>
/// A cast aimed at a point on the ground: walk until the point is within
/// CastRange, then cast at it.
/// </summary>
public class SkillCastingToPointBehaviour : SkillCastingBehaviourBase
{
    private Vector3 _castPoint;

    /// <summary>
    /// Only a cast aimed at a point. Casting into a target is a behaviour of its
    /// own and answers the same order with its own arguments.
    /// </summary>
    public override bool CanHandle(EventArgs args)
    {
        return args is SkillCastToPointActionStartedEventArgs pointArgs
            && (pointArgs.UnitSkill.Skill as ActiveSkill)?.Action is CastToPointAction;
    }

    protected override void ReadAim(EventArgs args)
    {
        _castPoint = (args as SkillCastToPointActionStartedEventArgs).Point;
    }

    protected override bool IsAimInCastRange()
    {
        return Vector3.Distance(transform.position, _castPoint) <= Skill.CastRange;
    }

    protected override void MoveToAim()
    {
        NavmeshMovement.Go(_castPoint);
    }

    protected override SkillParams BuildSkillParams()
    {
        return new SkillParams { Owner = gameObject, CastPoint = _castPoint };
    }
}
