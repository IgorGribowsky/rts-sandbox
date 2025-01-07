using Assets.Scripts.Infrastructure.Constants;
using Assets.Scripts.Infrastructure.Events;
using UnityEngine;

public class AutoAttackIdleBehaviour : AMovementBehaviour
{
    protected bool _damageReceivedFlag = false;
    protected float _damageReceivedAgressionTimer = 0f;
    protected bool _returningBackFlag = false;
    protected override void AdditionalAwake()
    {
        _unitEventManager.CalledToAttack += OnCalledToAttackHandler;
    }

    protected override void UpdateAction()
    {
        if (!_damageReceivedFlag && !_returningBackFlag)
        {
            FindNearestTargetAndAct();
        }

        if (!_triggeredOnEnemy)
        {
            IfNoTargetUpdate();
        }
        else
        {
            IfTargetExistsUpdate();
        }
    }

    private void OnCalledToAttackHandler(CalledToAttackEventArgs args)
    {
        if (IsActive && !_triggeredOnEnemy && !_returningBackFlag)
        {
            _triggeredOnEnemy = true;
            _damageReceivedFlag = true;
            _damageReceivedAgressionTimer = Constants.DamageReceivedAgressionTime;
            _currentTarget = args.Target;
            IfTargetFoundThen(args.Target);
        }
    }

    protected override void IfNoTargetUpdate()
    {
        var differenceVector = _movePoint - transform.position;
        var destinationDifVector = _navmeshMovement.Destination - _movePoint;
        differenceVector.y = 0;
        if (differenceVector.magnitude <= _navmeshMovement.StoppingDistance)
        {
            _returningBackFlag = false;
            _navmeshMovement.Stop();
        }
        else if (destinationDifVector.magnitude >= _navmeshMovement.StoppingDistance)
        {
            _navmeshMovement.Go(_movePoint);
        }
    }

    protected virtual void IfTargetExistsUpdate()
    {
        var distanceToPath = (gameObject.transform.position - _movePoint).magnitude;

        if (distanceToPath > Constants.PersecutionDistance)
        {
            _triggeredOnEnemy = false;
            _currentTarget = null;
            _returningBackFlag = true;
            _attackBehaviour.IsActive = false;
            _navmeshMovement.Go(_movePoint);
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
}
