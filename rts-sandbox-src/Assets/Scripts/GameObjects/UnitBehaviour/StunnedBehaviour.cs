using Assets.Scripts.GameObjects.UnitBehaviour;
using System;

/// <summary>
/// The unit is stunned and does nothing (M-019). It is an ordinary behaviour, so
/// switching to it smothers whatever the unit was doing exactly the way any other
/// order does — that is what breaks a cast in progress and stops a walk, and it
/// is why the stun needed no special case in the behaviour manager.
///
/// Never given by an order: the only thing that turns it on is StunEffect, and
/// the only thing that turns it off is that effect going away.
/// </summary>
public class StunnedBehaviour : UnitBehaviourBase
{
    private NavMeshMovement _navmeshMovement;

    public override UnitActionType Trigger => UnitActionType.Stun;

    protected override void OnInitialize()
    {
        _navmeshMovement = gameObject.GetComponent<NavMeshMovement>();
    }

    public override void StartAction(EventArgs args)
    {
        // Nothing is ever waited for here, so the end event would only confuse the
        // command queue: the queue is held by the stun itself, not by this.
        DisableTriggerEndEvent();

        if (_navmeshMovement != null)
        {
            _navmeshMovement.Stop();
        }
    }

    protected override void UpdateAction()
    {
    }
}
