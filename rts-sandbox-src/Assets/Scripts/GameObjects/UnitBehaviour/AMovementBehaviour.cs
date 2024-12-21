using Assets.Scripts.GameObjects.UnitBehaviour;
using Assets.Scripts.Infrastructure.Events;
using Assets.Scripts.Infrastructure.Helpers;
using System;
using System.Collections.Generic;
using UnityEngine;

public class AMovementBehaviour : UnitBehaviourBase
{
    private NavMeshMovement _navmeshMovement;
    private UnitEventManager _unitEventManager;
    private UnitValues _unitValues;
    private TeamMember _teamMember;
    private TeamController _teamController;
    private UnitBehaviourManager _unitBehaviourManager;
    private AttackingBehaviourBase _attackBehaviour;

    private bool _triggeredOnEnemy = false;
    private GameObject _currentTarget = null;
    private Vector3 _movePoint;

    public void Awake()
    {
        _navmeshMovement = gameObject.GetComponent<NavMeshMovement>();
        _unitEventManager = GetComponent<UnitEventManager>();
        _unitValues = GetComponent<UnitValues>();
        _teamMember = GetComponent<TeamMember>();
        _teamController = GameObject.FindGameObjectWithTag("GameController")
            .GetComponent<TeamController>();
        _unitBehaviourManager = gameObject.GetComponent<UnitBehaviourManager>();

        _attackBehaviour = gameObject.GetComponent<RangeAttackingBehaviour>();
        if (_attackBehaviour == null)
        {
            _attackBehaviour = gameObject.GetComponent<MeleeAttackingBehaviour>();
        }
    }

    public override void StartAction(EventArgs args)
    {
        var actionArgs = args as MoveActionStartedEventArgs;

        _movePoint = actionArgs.MovePoint;
        _navmeshMovement.Go(_movePoint);

        _triggeredOnEnemy = false;
        _currentTarget = null;
    }

    protected override void UpdateAction()
    {
        if (_attackBehaviour != null)
        {
            var enemyTeamIds = _teamController.GetEnemyTeams(_teamMember.TeamId);

            var target = gameObject.GetNearestUnitInRadius(_unitValues.AutoAttackDistance, unit =>
            {
                var teamMember = unit.GetComponent<TeamMember>();
                return teamMember != null && enemyTeamIds.Contains(teamMember.TeamId);
            });

            if (target == null)
            {
                _triggeredOnEnemy = false;
                _navmeshMovement.Go(_movePoint);
            }
            else if (_currentTarget == null || target != _currentTarget)
            {
                _triggeredOnEnemy = true;
                _attackBehaviour.StartAction(new AttackActionStartedEventArgs(target));
                _attackBehaviour.DisableTriggerEndEvent();
                _attackBehaviour.IsActive = true;
            }

            _currentTarget = target;
        }

        if (!_triggeredOnEnemy)
        {
            var differenceVector = _navmeshMovement.Destination - transform.position;
            differenceVector.y = 0;
            if (differenceVector.magnitude <= _navmeshMovement.StoppingDistance)
            {
                IsActive = false;
                _navmeshMovement.Stop();
                if (TriggerEndEventFlag)
                {
                    _unitEventManager.OnAMoveActionEnded();
                }
            }
        }
    }
}
