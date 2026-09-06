using Assets.Scripts.GameObjects.UnitBehaviour;
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
            var enemyTeamIds = _teamController.GetEnemyTeams(_teamMember.TeamId);

            var target = gameObject.GetNearestUnitInRadius(_unitValues.AutoAttackDistance, unit =>
            {
                var teamMember = unit.GetComponent<TeamMember>();
                return teamMember != null && enemyTeamIds.Contains(teamMember.TeamId) && unit.CanBeAttacked(_unitValues.DamageType);
            });

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
