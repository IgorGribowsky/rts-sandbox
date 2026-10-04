using Assets.Scripts.GameObjects.UnitBehaviour;
using Assets.SkillsSection.Scripts;
using Assets.SkillsSection.Scripts.Events;
using System;
using UnityEngine;
using static Assets.SkillsSection.Scripts.UnitSkills;

/// <summary>
/// What every cast does the same way: walk up until the aim is within CastRange,
/// stand still, count CastDuration, apply the skill, put it on cooldown. What
/// differs between a cast at a point and a cast at a unit is only the aim, and
/// that is what the abstract members below stand for.
///
/// Both casts answer the same SkillCast order and are told apart by CanHandle,
/// exactly as M-005 describes.
/// </summary>
public abstract class SkillCastingBehaviourBase : UnitBehaviourBase
{
    protected NavMeshMovement NavmeshMovement;
    protected UnitEventManager UnitEvents;
    protected UnitSkills UnitSkillsScript;

    private UnitValues _unitValues;

    protected UnitSkill CastedSkill;
    protected ActiveSkill Skill;

    private bool _castIsProcessing;
    private float _castAnimation;

    public override UnitActionType Trigger => UnitActionType.SkillCast;

    protected override void OnInitialize()
    {
        NavmeshMovement = gameObject.GetComponent<NavMeshMovement>();
        UnitEvents = GetComponent<UnitEventManager>();
        UnitSkillsScript = gameObject.GetComponent<UnitSkills>();
        _unitValues = gameObject.GetComponent<UnitValues>();
    }

    public override void StartAction(EventArgs args)
    {
        EnableTriggerEndEvent();

        var actionArgs = args as SkillCastActionStartedEventArgs;

        CastedSkill = actionArgs.UnitSkill;
        Skill = CastedSkill.Skill as ActiveSkill;

        ReadAim(args);
    }

    protected override void PreUpdate()
    {
        if (!IsActive)
        {
            _castIsProcessing = false;
            _castAnimation = 0;
        }
    }

    protected override void UpdateAction()
    {
        // The aim can go away mid-cast: a target dies while the caster walks up.
        if (!IsAimStillValid())
        {
            StopAction();
            return;
        }

        if (!_castIsProcessing)
        {
            if (!IsAimInCastRange())
            {
                MoveToAim();
            }
            else if (!UnitSkillsScript.CheckIfCanCast(CastedSkill))
            {
                StopAction();
                return;
            }
            else
            {
                NavmeshMovement.Stop();

                // The cast starts once the caster looks at the aim (T-033).
                if (Facing.TurnTowards(transform, AimPoint, _unitValues.TurnSpeed))
                {
                    _castIsProcessing = true;
                }
            }
        }

        if (_castIsProcessing)
        {
            // Close the last few degrees of the tolerance, and follow a target that moves.
            Facing.TurnTowards(transform, AimPoint, _unitValues.TurnSpeed);

            _castAnimation += Time.deltaTime;

            if (_castAnimation > Skill.CastDuration)
            {
                if (UnitSkillsScript.CheckIfCanCast(CastedSkill))
                {
                    UnitSkillsScript.Cast(CastedSkill, BuildSkillParams());
                }

                _castIsProcessing = false;
                StopAction();
            }
        }
    }

    /// <summary>Pulls the aim — a point or a unit — out of the order's arguments.</summary>
    protected abstract void ReadAim(EventArgs args);

    protected abstract bool IsAimInCastRange();

    /// <summary>Where the caster turns to before the cast starts (T-033).</summary>
    protected abstract Vector3 AimPoint { get; }

    protected abstract void MoveToAim();

    protected abstract SkillParams BuildSkillParams();

    /// <summary>A point never goes away; a target can die. Default is "still there".</summary>
    protected virtual bool IsAimStillValid() => true;

    protected void StopAction()
    {
        IsActive = false;

        if (TriggerEndEventFlag)
        {
            UnitEvents.OnSkillCastActionEnded();
        }
    }
}
