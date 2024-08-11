using Assets.Scripts.Infrastructure.Events;
using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class MeleeAttacking : MonoBehaviour
{
    private NavMeshAgent _navmeshAgent;
    private UnitEventManager _unitEventManager;
    private UnitValues _unitValues;

    private GameObject target = null;
    private UnitEventManager targetEventManager = null;
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
        target = args.Target;
        _navmeshAgent.avoidancePriority = 90;
        targetEventManager = target.GetComponent<UnitEventManager>();
        isProcessing = true;
    }

    protected void Stop(EventArgs args)
    {
        if (isProcessing)
        {
            target = null;
            _navmeshAgent.avoidancePriority = 50;
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
            if (target != null)
            {
                var distanceToTarget = (transform.position - target.transform.position).magnitude;

                if (!attackIsProcessing && distanceToTarget > _unitValues.MeleeAttackDistance)
                {
                    _navmeshAgent.destination = target.transform.position;
                }
                else
                {
                    _navmeshAgent.destination = transform.position;
                }

                if (distanceToTarget < _unitValues.MeleeAttackDistance 
                    && attackCD <= 0
                    && !attackIsProcessing)
                {
                    attackIsProcessing = true;
                    attackCD = _unitValues.AttackRate;
                }

                if (attackIsProcessing)
                {
                    if (distanceToTarget < _unitValues.MeleeAttackDistance * _unitValues.MeleeAttackMaxDistancePercent)
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
                        targetEventManager.OnDamageReceived(gameObject, _unitValues.Damage, _unitValues.DamageType);

                        attackIsProcessing = false;
                        attackAnimation = 0;
                    }
                }
            }
            else
            {
                Stop(new EventArgs());
                _unitEventManager.OnMoveActionEnded();
            }
        }
    }
}
