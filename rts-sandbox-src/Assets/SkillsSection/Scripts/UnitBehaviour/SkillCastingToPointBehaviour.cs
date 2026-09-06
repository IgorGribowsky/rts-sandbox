using Assets.Scripts.GameObjects.UnitBehaviour;
using Assets.SkillsSection.Scripts;
using Assets.SkillsSection.Scripts.Events;
using System;
using UnityEngine;
using static Assets.SkillsSection.Scripts.UnitSkills;

public class SkillCastingToPointBehaviour : UnitBehaviourBase
{
    private NavMeshMovement _navmeshMovement;
    private UnitEventManager _unitEventManager;
    private UnitSkills _unitSkills;

    private UnitSkill unitSkill;
    private ActiveSkill skill;
    private CastToPointAction castAction;
    private Vector3 castPoint;

    private bool castIsProcessing = false;
    private bool isMoving = false;
    private float castAnimation = 0;

    public override UnitActionType Trigger => UnitActionType.SkillCast;

    protected override void OnInitialize()
    {
        _navmeshMovement = gameObject.GetComponent<NavMeshMovement>();
        _unitEventManager = GetComponent<UnitEventManager>();
        _unitSkills = gameObject.GetComponent<UnitSkills>();
    }

    /// <summary>
    /// Only a cast aimed at a point. Casting into a target is a behaviour of its
    /// own and will answer the same order with its own arguments.
    /// </summary>
    public override bool CanHandle(EventArgs args)
    {
        return args is SkillCastToPointActionStartedEventArgs pointArgs
            && (pointArgs.UnitSkill.Skill as ActiveSkill)?.Action is CastToPointAction;
    }

    public override void StartAction(EventArgs args)
    {
        EnableTriggerEndEvent();

        var actionArgs = args as SkillCastToPointActionStartedEventArgs;

        unitSkill = actionArgs.UnitSkill;
        skill = unitSkill.Skill as ActiveSkill;
        castAction = skill.Action as CastToPointAction;

        castPoint = actionArgs.Point;
    }

    protected override void PreUpdate()
    {
        //CHECK HERE AND IN MeleeAttackingBehaviour in profiler mb this code is eating fps
        if (!IsActive)
        {
            castIsProcessing = false;
            isMoving = false;
            castAnimation = 0;
        }
    }

    protected override void UpdateAction()
    {
        var distanceToTarget = Vector3.Distance(transform.position, castPoint);

        if (!castIsProcessing)
        {
            if (distanceToTarget > skill.CastRange)
            {
                _navmeshMovement.Go(castPoint);
                isMoving = true;
            }
            else if (!_unitSkills.CheckIfCanCast(unitSkill))
            {
                StopAction();
            }
            else
            {
                _navmeshMovement.Stop();
                castIsProcessing = true;
                isMoving = false;
            }
        }

        if (castIsProcessing)
        {
            castAnimation += Time.deltaTime;

            if (castAnimation > skill.CastDuration)
            {
                if (_unitSkills.CheckIfCanCast(unitSkill))
                {
                    _unitSkills.Cast(unitSkill, new SkillParams { Owner = gameObject, CastPoint = castPoint});
                }
                castIsProcessing = false;
                StopAction();
            }
        }
    }

    private void StopAction()
    {
        IsActive = false;

        if (TriggerEndEventFlag)
        {
            _unitEventManager.OnMoveActionEnded();
        }
    }
}
