using Assets.Scripts.GameObjects.UnitBehaviour;
using Assets.Scripts.Infrastructure.Constants;
using Assets.Scripts.Infrastructure.Enums;
using Assets.Scripts.Infrastructure.Events;
using Assets.Scripts.Infrastructure.Helpers;
using System;
using UnityEngine;

public abstract class AutoAttackingBehaviourBase : UnitBehaviourBase
{
    protected NavMeshMovement _navmeshMovement;
    protected UnitEventManager _unitEventManager;
    protected UnitValues _unitValues;
    protected TeamMember _teamMember;
    protected TeamController _teamController;
    protected UnitBehaviourManager _unitBehaviourManager;
    protected AttackingBehaviourBase _attackBehaviour;
    protected bool _triggeredOnEnemy = false;
    protected GameObject _currentTarget = null;
    protected Vector3 _movePoint;

    /// <summary>This unit as the registry sees it: components and size, ready.</summary>
    private UnitRecord _self;

    /// <summary>Frames left until the next search, see GameConstants.</summary>
    private int _framesUntilSearch;

    protected override void OnInitialize()
    {
        _navmeshMovement = gameObject.GetComponent<NavMeshMovement>();
        _unitEventManager = GetComponent<UnitEventManager>();
        _unitValues = GetComponent<UnitValues>();
        _teamMember = GetComponent<TeamMember>();
        _teamController = GameObject.FindGameObjectWithTag(Tag.GameController.ToString())
            .GetComponent<TeamController>();
        _unitBehaviourManager = Context.Manager;

        // The very same behaviour the explicit attack order runs, on purpose:
        // auto attack drives it while staying active itself.
        _attackBehaviour = _unitBehaviourManager.GetForAction(UnitActionType.Attack) as AttackingBehaviourBase;

        _self = UnitRegistry.Of(gameObject);

        // Spread the units across frames, so a hundred of them do not all search
        // on the same one.
        _framesUntilSearch = Mathf.Abs(gameObject.GetInstanceID()) % GameConstants.TargetSearchFrameInterval;

        AdditionalInitialize();
    }

    public override void StartAction(EventArgs args)
    {
        var movePoint = GetMovePoint(args);
        if (movePoint.HasValue && _navmeshMovement != null)
        {
            _movePoint = movePoint.Value;
            _navmeshMovement.Go(_movePoint);
        }

        _triggeredOnEnemy = false;
        _currentTarget = null;
    }

    /// <summary>
    /// Going to a point comes either from an A-move order or from going idle.
    /// </summary>
    private Vector3? GetMovePoint(EventArgs args)
    {
        if (args is MoveActionStartedEventArgs moveArgs)
        {
            return moveArgs.MovePoint;
        }

        if (args is AutoAttackIdleStartedEventArgs idleArgs)
        {
            return idleArgs.MovePoint;
        }

        return null;
    }

    protected override void OnDeactivated()
    {
        if (_attackBehaviour != null)
        {
            _attackBehaviour.Deactivate();
        }
    }

    protected abstract override void UpdateAction();

    protected abstract void IfNoTargetUpdate();

    protected abstract void IfTargetExistsUpdate();

    protected virtual void AdditionalInitialize() { }

    protected virtual void FindNearestTargetAndAct()
    {
        if (_attackBehaviour != null)
        {
            // The search is the most expensive thing a unit does, so it runs
            // once in a few frames. A target that just died is the exception:
            // that one is dropped immediately, or the unit keeps swinging at a
            // corpse until its turn comes round.
            var targetLost = _triggeredOnEnemy && _currentTarget == null;

            if (!targetLost)
            {
                if (_framesUntilSearch > 0)
                {
                    _framesUntilSearch--;
                    return;
                }
            }

            _framesUntilSearch = GameConstants.TargetSearchFrameInterval;

            if (_self == null || _self.GameObject == null)
            {
                _self = UnitRegistry.Of(gameObject);
            }

            var enemyTeamIds = _teamController.GetEnemyTeams(_teamMember.TeamId);

            var target = UnitRegistry.FindNearestOfTeams(
                _self,
                _unitValues.AutoAttackDistance,
                enemyTeamIds,
                candidate => candidate.Values == null || !candidate.Values.IsInvulnerable);

            if (target == null)
            {
                if (_triggeredOnEnemy)
                {
                    _triggeredOnEnemy = false;
                    _attackBehaviour.IsActive = false;
                    IfTargetNotFoundThen();
                }
            }
            else if (_currentTarget == null || target != _currentTarget)
            {
                _triggeredOnEnemy = true;
                IfTargetFoundThen(target);
            }

            _currentTarget = target;
        }
    }

    protected virtual void IfTargetFoundThen(GameObject target)
    {
        _attackBehaviour.StartAction(new AttackActionStartedEventArgs(target));
        _attackBehaviour.DisableTriggerEndEvent();
        _attackBehaviour.IsActive = true;
    }

    protected virtual void IfTargetNotFoundThen()
    {
        if (_navmeshMovement != null)
        {
            _navmeshMovement.Go(_movePoint);
        }
    }
}
