using Assets.Scripts.GameObjects.UnitBehaviour;
using Assets.Scripts.Infrastructure.Constants;
using Assets.Scripts.Infrastructure.Events;
using Assets.Scripts.Infrastructure.Helpers;
using System;
using UnityEngine;

public class AMovementBehaviour : UnitBehaviourBase
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

    protected bool _damageReceivedFlag = false;
    protected float _damageReceivedAgressionTimer = 0f;

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

        _unitEventManager.CalledToAttack += OnCalledToAttackHandler;
    }

    public void OnCalledToAttackHandler(CalledToAttackEventArgs args)
    {
        if (IsActive && !_triggeredOnEnemy)
        {
            _triggeredOnEnemy = true;
            _damageReceivedFlag = true;
            _damageReceivedAgressionTimer = Constants.DamageReceivedAgressionTime;
            _currentTarget = args.Target;
            IfTargetFoundThen(args.Target);
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
        if (!_damageReceivedFlag)
        {
            FindNearestTargetAndAct();
        }

        if (!_triggeredOnEnemy)
        {
            IfNoTargetUpdate();
        }
    }

    protected override void PostUpdate()
    {
        base.PostUpdate();
        if (_damageReceivedFlag)
        {
            _damageReceivedAgressionTimer -= Time.deltaTime;
            if (_damageReceivedAgressionTimer <= 0)
            {
                _damageReceivedFlag = false;
            }
        }
    }

    protected virtual void IfNoTargetUpdate()
    {
        var differenceVector = _movePoint - transform.position;
        var destinationDifVector = _navmeshMovement.Destination - _movePoint;
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
        else if (destinationDifVector.magnitude >= _navmeshMovement.StoppingDistance)
        {
            _navmeshMovement.Go(_movePoint);
        }
    }

    protected virtual void FindNearestTargetAndAct()
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
                if (_triggeredOnEnemy)
                {
                    _triggeredOnEnemy = false;
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
        _navmeshMovement.Go(_movePoint);
    }
}
