using Assets.Scripts.Infrastructure.Helpers;
using Assets.SkillsSection.Scripts.Events;
using System;
using UnityEngine;

/// <summary>
/// A cast aimed at a unit: walk up to the target, then cast at it. The target is
/// checked every frame — it can die or be destroyed while the caster is walking.
/// </summary>
public class SkillCastingToTargetBehaviour : SkillCastingBehaviourBase
{
    private GameObject _target;

    /// <summary>
    /// Only a cast aimed at a unit. A cast at a point answers the same order with
    /// its own arguments.
    /// </summary>
    public override bool CanHandle(EventArgs args)
    {
        return args is SkillCastToTargetActionStartedEventArgs targetArgs
            && (targetArgs.UnitSkill.Skill as ActiveSkill)?.Action is CastToTargetAction;
    }

    protected override void ReadAim(EventArgs args)
    {
        _target = (args as SkillCastToTargetActionStartedEventArgs).Target;
    }

    protected override bool IsAimStillValid() => _target != null;

    /// <summary>
    /// Distance between the surfaces, the same GetDistanceTo the melee attack and
    /// the follow behaviour use, so CastRange means the same thing everywhere.
    /// </summary>
    protected override bool IsAimInCastRange()
    {
        return gameObject.GetDistanceTo(_target) <= Skill.CastRange;
    }

    protected override void MoveToAim()
    {
        NavmeshMovement.GoToObject(_target, Skill.CastRange);
    }

    protected override SkillParams BuildSkillParams()
    {
        return new SkillParams { Owner = gameObject, Target = _target };
    }
}
