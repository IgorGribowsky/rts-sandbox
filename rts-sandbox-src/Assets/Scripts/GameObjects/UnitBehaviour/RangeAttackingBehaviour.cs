using Assets.Scripts;
using Assets.Scripts.GameObjects.Projectiles;
using Assets.Scripts.GameObjects.UnitBehaviour;
using Assets.Scripts.Infrastructure.Constants;
using Assets.Scripts.Infrastructure.Events;
using Assets.Scripts.Infrastructure.Helpers;
using System;
using UnityEngine;

public class RangeAttackingBehaviour : AttackingBehaviourBase
{
    private NavMeshMovement _navmeshMovement;
    private UnitEventManager _unitEventManager;
    private UnitValues _unitValues;

    private UnitEventManager _targetEventManager = null;
    private float attackCD = 0;
    private float attackAnimation = 0;
    private bool attackIsProcessing = false;

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

        Target = actionArgs.Target;
        _targetEventManager = Target.GetComponent<UnitEventManager>();
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
        if (Target == null)
        {
            IsActive = false;

            if (_navmeshMovement != null)
            {
                _navmeshMovement.Stop();
            }

            if (TriggerEndEventFlag)
            {
                _unitEventManager.OnAttackActionEnded();
            }
            return;
        }

        var distanceToTarget = gameObject.GetDistanceTo(Target);

        // A tower shoots where it stands, the way it always did; a unit turns
        // to the target first (T-033).
        var facesTarget = _navmeshMovement == null || _unitValues.IsBuilding;

        if (_navmeshMovement != null)
        {
            if (!attackIsProcessing && distanceToTarget > _unitValues.RangeAttackDistance)
            {
                _navmeshMovement.Go(Target.transform.position);
            }
            else
            {
                // Standing to shoot, the archer gives way to melee units going
                // for their places and never shoves them out of the fight (T-064).
                _navmeshMovement.Stop();
                _navmeshMovement.SetAvoidancePriority(GameConstants.RangeFireAvoidancePriority);

                if (!facesTarget)
                {
                    facesTarget = Facing.TurnTowards(transform, Target.transform.position, _unitValues.TurnSpeed);
                }
            }
        }


        if (distanceToTarget < _unitValues.RangeAttackDistance
            && attackCD <= 0
            && !attackIsProcessing
            && facesTarget)
        {
            attackIsProcessing = true;
            attackCD = _unitValues.CurrentAttackRate;
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

            if (attackAnimation >= _unitValues.CurrentAttackRate * _unitValues.AttackDurationPercent)
            {
                var projectile = Instantiate(_unitValues.RangeAttackProjectile, transform.position, transform.rotation);
                var projectileBehavior = projectile.GetComponent<ProjectileBehavior>();
                // Rolled as the arrow leaves; the numbers show when it lands (T-070).
                var damage = AttackRolls.Roll(gameObject, _unitValues.CurrentDamage, out var critical);
                projectileBehavior.SetProperties(Target, gameObject, _unitValues.ProjectileSpeed, damage, _unitValues.DamageType);
                projectileBehavior.IsCritical = critical;

                attackIsProcessing = false;
                attackAnimation = 0;
            }
        }
    }
}
