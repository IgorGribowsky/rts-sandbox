using Assets.Scripts.GameObjects.UnitBehaviour;
using System;

public class HoldingBehaviour : UnitBehaviourBase
{
    private NavMeshMovement _navmeshMovement;

    public override UnitActionType Trigger => UnitActionType.Hold;

    protected override void OnInitialize()
    {
        _navmeshMovement = gameObject.GetComponent<NavMeshMovement>();
    }

    public override void StartAction(EventArgs args)
    {
        _navmeshMovement.Stop();
    }

    protected override void UpdateAction()
    {
    }
}
