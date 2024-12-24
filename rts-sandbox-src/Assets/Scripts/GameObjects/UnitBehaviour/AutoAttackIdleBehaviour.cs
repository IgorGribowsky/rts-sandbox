public class AutoAttackIdleBehaviour : AMovementBehaviour
{
    protected override void IfNoTargetUpdate()
    {
        var differenceVector = _movePoint - transform.position;
        var destinationDifVector = _navmeshMovement.Destination - _movePoint;
        differenceVector.y = 0;
        if (differenceVector.magnitude <= _navmeshMovement.StoppingDistance)
        {
            _navmeshMovement.Stop();
        }
        else if (destinationDifVector.magnitude >= _navmeshMovement.StoppingDistance)
        {
            _navmeshMovement.Go(_movePoint);
        }
    }
}
