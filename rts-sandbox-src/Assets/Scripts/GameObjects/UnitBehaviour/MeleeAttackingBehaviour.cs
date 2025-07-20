using Assets.Scripts.GameObjects.UnitBehaviour;
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

    public void Awake()
    {
        _navmeshMovement = gameObject.GetComponent<NavMeshMovement>();
        _unitEventManager = GetComponent<UnitEventManager>();
        _unitValues = gameObject.GetComponent<UnitValues>();
    }

    public override void StartAction(EventArgs args)
    {
        EnableTriggerEndEvent();

        var actionArgs = args as AttackActionStartedEventArgs;

        Target = actionArgs.Target;
        _targetEventManager = Target.GetComponent<UnitEventManager>();
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

        if (!attackIsProcessing && distanceToTarget > _unitValues.MeleeAttackDistance)
        {
            _navmeshMovement.GoToObject(Target, _unitValues.MeleeAttackDistance);
            Debug.Log("GoToObject");
        }
        else
        {
            _navmeshMovement.Stop();
        }

        if (distanceToTarget < _unitValues.MeleeAttackDistance + 0.01f
            && attackCD <= 0
            && !attackIsProcessing)
        {
            Debug.Log("Attack started");
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
                Debug.Log("Attack rejected");
                attackIsProcessing = false;
                attackAnimation = 0;
            }

            if (attackAnimation >= _unitValues.AttackRate * _unitValues.AttackDurationPercent)
            {
                _targetEventManager.OnDamageReceived(gameObject, _unitValues.Damage, _unitValues.DamageType);

                Debug.Log("Attack finished");
                attackIsProcessing = false;
                attackAnimation = 0;
            }
        }
    }
}
