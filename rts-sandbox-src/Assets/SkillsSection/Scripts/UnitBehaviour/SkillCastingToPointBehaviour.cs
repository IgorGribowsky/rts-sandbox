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

    private bool castIsProcessing = false; 
    private bool isMoving = false;
    private float castAnimation = 0;

    public void Awake()
    {
        _navmeshMovement = gameObject.GetComponent<NavMeshMovement>();
        _unitEventManager = GetComponent<UnitEventManager>();
        _unitSkills = gameObject.GetComponent<UnitSkills>();
    }

    public override void StartAction(EventArgs args)
    {
        EnableTriggerEndEvent();

        var actionArgs = args as SkillCastToPointActionStartedEventArgs;

        unitSkill = actionArgs.UnitSkill;
        skill = unitSkill.Skill as ActiveSkill;
        castAction = skill.Action as CastToPointAction;

        castAction.CastPoint = actionArgs.Point;
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
        var distanceToTarget = Vector3.Distance(transform.position, castAction.CastPoint);

        if (!castIsProcessing)
        {
            if (distanceToTarget > skill.CastRange)
            {
                _navmeshMovement.Go(castAction.CastPoint);
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
                    _unitSkills.Cast(unitSkill);
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
