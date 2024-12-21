using Assets.Scripts.GameObjects.UnitBahaviour;
using Assets.Scripts.Infrastructure.Events;
using System;

public class MovementBehaviour : UnitBehaviourBase
{
    private NavMeshMovement _navmeshMovement;
    private UnitEventManager _unitEventManager;

    public void Awake()
    {
        _navmeshMovement = gameObject.GetComponent<NavMeshMovement>();
        _unitEventManager = GetComponent<UnitEventManager>();
    }

    public override void StartAction(EventArgs args)
    {
        var actionArgs = args as MoveActionStartedEventArgs;

        _navmeshMovement.Go(actionArgs.MovePoint);
    }

    protected override void UpdateAction()
    {
        var differenceVector = _navmeshMovement.Destination - transform.position;
        differenceVector.y = 0;
        if (differenceVector.magnitude <= _navmeshMovement.StoppingDistance)
        {
            IsActive = false;
            _navmeshMovement.Stop();
            _unitEventManager.OnMoveActionEnded();
        }
    }
}
