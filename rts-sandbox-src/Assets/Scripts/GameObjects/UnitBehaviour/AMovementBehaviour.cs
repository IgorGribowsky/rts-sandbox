using Assets.Scripts.GameObjects.UnitBehaviour;

public class AMovementBehaviour : AutoAttackingBehaviourBase
{
    public override UnitActionType Trigger => UnitActionType.AMove;

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
    }

    protected override void IfTargetExistsUpdate()
    {
        // Specific logic can be added here if needed
    }
}
