using Assets.Scripts.GameObjects.Projectiles;
using Assets.Scripts.Infrastructure.Events;
using Assets.Scripts.Infrastructure.Helpers;
using System;
using UnityEngine;
using UnityEngine.AI;

public class RangeAttacking : MonoBehaviour
{
    private NavMeshAgent _navmeshAgent;
    private UnitEventManager _unitEventManager;
    private UnitValues _unitValues;

    private GameObject _target = null;
    private UnitEventManager _targetEventManager = null;
    private float attackCD = 0;
    private float attackAnimation = 0;
    private bool attackIsProcessing = false;
    private bool isProcessing = false;

    void Start()
    {
        _navmeshAgent = gameObject.GetComponent<NavMeshAgent>();
        _unitValues = gameObject.GetComponent<UnitValues>();

        _navmeshAgent.speed = _unitValues.MovementSpeed;

        _unitEventManager = GetComponent<UnitEventManager>();

        _unitEventManager.AttackActionStarted += Attack;
        _unitEventManager.FollowActionStarted += Stop;
        _unitEventManager.MoveActionStarted += Stop;

    }

    protected void Attack(AttackActionStartedEventArgs args)
    {
        _target = args.Target;
        _navmeshAgent.avoidancePriority = 90;
        _targetEventManager = _target.GetComponent<UnitEventManager>();
        isProcessing = true;
    }

    protected void Stop(EventArgs args)
    {
        if (isProcessing)
        {
            _target = null;
            _navmeshAgent.avoidancePriority = 50;
            _navmeshAgent.destination = gameObject.transform.position;
            isProcessing = false;
        }
    }

    void Update()
    {
        if (attackCD > 0)
        {
            attackCD -= Time.deltaTime;
        }

        if (isProcessing)
        {
            if (_target != null)
            {
                var distanceToTarget = DistanceHelper.GetDistanceBetweenObjects(gameObject, _target);

                if (!attackIsProcessing && distanceToTarget > _unitValues.RangeAttackDistance)
                {
                    _navmeshAgent.destination = _target.transform.position;
                }
                else
                {
                    _navmeshAgent.destination = transform.position;
                }

                if (distanceToTarget < _unitValues.RangeAttackDistance
                    && attackCD <= 0
                    && !attackIsProcessing)
                {
                    attackIsProcessing = true;
                    attackCD = _unitValues.AttackRate;
                }

                if (attackIsProcessing)
                {
                    if (distanceToTarget < _unitValues.RangeAttackDistance + _unitValues.AttackBreakDistance)
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
                        var projectile = Instantiate(_unitValues.RangeAttackProjectile, transform.position, transform.rotation);
                        var projectileBehavior = projectile.GetComponent<ProjectileBehavior>();
                        projectileBehavior.SetProperties(_target, _unitValues.ProjectileSpeed, _unitValues.Damage, _unitValues.DamageType);

                        attackIsProcessing = false;
                        attackAnimation = 0;
                    }
                }
            }
            else
            {
                Stop(new EventArgs());
                _unitEventManager.OnAttackActionEnded();
            }
        }
    }
}
