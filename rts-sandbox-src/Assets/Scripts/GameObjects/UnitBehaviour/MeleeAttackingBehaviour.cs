using Assets.Scripts.GameObjects.UnitBehaviour;
using Assets.Scripts.Infrastructure.Constants;
using Assets.Scripts.Infrastructure.Events;
using Assets.Scripts.Infrastructure.Helpers;
using System;
using UnityEngine;

public class MeleeAttackingBehaviour : AttackingBehaviourBase
{
    private NavMeshMovement _navmeshMovement;
    private UnitEventManager _unitEventManager;
    private UnitValues _unitValues;

    private UnitEventManager _targetEventManager = null;
    private float attackCD = 0;
    private float attackAnimation = 0;
    private bool attackIsProcessing = false;

    // The target this unit holds a place around (ApproachSlots), to give it back.
    private GameObject _placeTarget;

    protected override void OnInitialize()
    {
        _navmeshMovement = gameObject.GetComponent<NavMeshMovement>();
        _unitEventManager = GetComponent<UnitEventManager>();
        _unitValues = gameObject.GetComponent<UnitValues>();
    }

    public override void StartAction(EventArgs args)
    {
        EnableTriggerEndEvent();

        var actionArgs = args as AttackActionStartedEventArgs;

        if (_placeTarget != null && _placeTarget != actionArgs.Target)
        {
            ReleasePlace();
        }

        Target = actionArgs.Target;
        _targetEventManager = Target.GetComponent<UnitEventManager>();
    }

    /// <summary>
    /// The gap to keep from the target at the place: a little inside the
    /// attack distance, because the agent brakes stoppingDistance short.
    /// </summary>
    private float PlaceDistance =>
        Mathf.Max(0f, _unitValues.MeleeAttackDistance - _navmeshMovement.StoppingDistance - 0.1f);

    /// <summary>
    /// Is there a free place for this unit around the target. Auto attack asks
    /// it to prefer an enemy that can still be surrounded (T-064).
    /// </summary>
    public bool HasRoomAt(GameObject target)
    {
        var ring = target.GetSize() + _navmeshMovement.Size + PlaceDistance;
        return ApproachSlots.HasRoomFor(target, gameObject, ring, _navmeshMovement.Size);
    }

    /// <summary>
    /// To a place of its own around the target, so a group surrounds an enemy
    /// instead of piling up on one side. No place left: the unit still goes for
    /// the target the old way and waits behind the others — a target the player
    /// chose stays the target; one auto attack chose is changed by its next
    /// search for an enemy with room (AutoAttackingBehaviourBase).
    /// </summary>
    private void ApproachTarget()
    {
        if (_navmeshMovement.TryGoToPlaceAt(Target, PlaceDistance))
        {
            _placeTarget = Target;
            _navmeshMovement.SetAvoidancePriority(GameConstants.MeleeFightAvoidancePriority);
            return;
        }

        _navmeshMovement.GoToObject(Target, _unitValues.MeleeAttackDistance);
        _navmeshMovement.SetAvoidancePriority(GameConstants.MeleeNoPlaceAvoidancePriority);
    }

    private void ReleasePlace()
    {
        _navmeshMovement.ReleasePlaceAt(_placeTarget);
        _placeTarget = null;
    }

    protected override void OnDeactivated()
    {
        ReleasePlace();
    }

    protected override void PreUpdate()
    {
        if (attackCD > 0)
        {
            attackCD -= Time.deltaTime;
        }

        if (!IsActive)
        {
            attackIsProcessing = false;
            attackAnimation = 0;

            // Auto attack switches this behaviour off by the flag, without
            // Deactivate, so the place is given back here too.
            if (_placeTarget != null)
            {
                ReleasePlace();
            }
        }
    }

    protected override void UpdateAction()
    {
        if (Target == null)
        {
            IsActive = false;
            _navmeshMovement.Stop();
            if (TriggerEndEventFlag)
            {
                _unitEventManager.OnAttackActionEnded();
            }
            return;
        }

        var distanceToTarget = gameObject.GetDistanceTo(Target);
        var facesTarget = false;

        if (!attackIsProcessing && distanceToTarget > _unitValues.MeleeAttackDistance)
        {
            ApproachTarget();
        }
        else
        {
            _navmeshMovement.Stop();
            _navmeshMovement.SetAvoidancePriority(GameConstants.MeleeFightAvoidancePriority);

            // No blow with the back or the side to the enemy (T-033).
            facesTarget = Facing.TurnTowards(transform, Target.transform.position, _unitValues.TurnSpeed);
        }

        if (distanceToTarget < _unitValues.MeleeAttackDistance + 0.01f
            && attackCD <= 0
            && !attackIsProcessing
            && facesTarget)
        {
            attackIsProcessing = true;
            attackCD = _unitValues.AttackRate;
        }
        if (attackIsProcessing)
        {
            if (distanceToTarget < _unitValues.MeleeAttackDistance + _unitValues.AttackBreakDistance)
            {
                attackAnimation += Time.deltaTime;
            }
            else
            {
                attackIsProcessing = false;
                attackAnimation = 0;
            }

            if (attackAnimation >= _unitValues.AttackRate * _unitValues.AttackDurationPercent)
            {
                _targetEventManager.OnDamageReceived(gameObject, _unitValues.Damage, _unitValues.DamageType);

                // A new kind of ordinary attack has to raise this too, otherwise
                // on-hit passives stay silent for it.
                _unitEventManager.OnDamageDealt(Target, _unitValues.Damage, _unitValues.DamageType);

                attackIsProcessing = false;
                attackAnimation = 0;
            }
        }
    }
}
