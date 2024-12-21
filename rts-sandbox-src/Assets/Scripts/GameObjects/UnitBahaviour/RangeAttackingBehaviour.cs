using Assets.Scripts.GameObjects.Projectiles;
using Assets.Scripts.GameObjects.UnitBahaviour;
using Assets.Scripts.Infrastructure.Events;
using Assets.Scripts.Infrastructure.Helpers;
using System;
using UnityEngine;

public class RangeAttackingBehaviour : UnitBehaviourBase
{
    private NavMeshMovement _navmeshMovement;
    private UnitEventManager _unitEventManager;
    private UnitValues _unitValues;

    private GameObject _target = null;
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
        var actionArgs = args as AttackActionStartedEventArgs;

        _target = actionArgs.Target;
        _targetEventManager = _target.GetComponent<UnitEventManager>();
    }

    protected override void PreUpdate()
    {
        if (attackCD > 0)
        {
            attackCD -= Time.deltaTime;
        }
    }

    protected override void UpdateAction()
    {
        if (_target == null)
        {
            IsActive = false;
            _navmeshMovement.Stop();
            _unitEventManager.OnAttackActionEnded();
            return;
        }

        var distanceToTarget = gameObject.GetDistanceTo(_target);

        if (!attackIsProcessing && distanceToTarget > _unitValues.RangeAttackDistance)
        {
            _navmeshMovement.Go(_target.transform.position);
        }
        else
        {
            _navmeshMovement.Stop();
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
                projectileBehavior.SetProperties(_target, gameObject, _unitValues.ProjectileSpeed, _unitValues.Damage, _unitValues.DamageType);

                attackIsProcessing = false;
                attackAnimation = 0;
            }
        }
    }
}
