public class AMovementBehaviour : AutoAttackingBehaviourBase
{
    protected override void UpdateAction()
    {
        FindNearestTargetAndAct();

        if (!_triggeredOnEnemy)
        {
            IfNoTargetUpdate();
        }
        else
        {
            IfTargetExistsUpdate();
        }
    }

    protected override void IfNoTargetUpdate()
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

    protected override void IfTargetExistsUpdate()
    {
        // Specific logic can be added here if needed
    }
}
