public class AutoAttackIdleBehaviour : AMovementBehaviour
{
    protected virtual void IfNoTargetUpdate()
    {
        var differenceVector = _navmeshMovement.Destination - transform.position;
        differenceVector.y = 0;
        if (differenceVector.magnitude <= _navmeshMovement.StoppingDistance)
        {
            _navmeshMovement.Stop();
        }
    }
}
