using Assets.Scripts.GameObjects.UnitBehaviour;
using Assets.Scripts.Infrastructure.Constants;
using Assets.Scripts.Infrastructure.Events;
using System;
using UnityEngine;

public class FollowingBehaviour : UnitBehaviourBase
{
    private NavMeshMovement _navmeshMovement;
    private UnitEventManager _unitEventManager;

    private GameObject target = null;

    public void Awake()
    {
        _navmeshMovement = gameObject.GetComponent<NavMeshMovement>();
        _unitEventManager = GetComponent<UnitEventManager>();
    }

    public override void StartAction(EventArgs args)
    {
        EnableTriggerEndEvent();

        var actionArgs = args as FollowActionStartedEventArgs;

        target = actionArgs.Target;
    }

    protected override void UpdateAction()
    {
        if (target == null)
        {
            IsActive = false;
            _navmeshMovement.Stop();

            if (TriggerEndEventFlag)
            {
                _unitEventManager.OnFollowActionEnded();
            }
            return;
        }

        var distanceToTarget = (transform.position - target.transform.position).magnitude;
        if (distanceToTarget > GameConstants.FollowingDistance)
        {
            _navmeshMovement.Go(target.transform.position);
        }
        else
        {
            _navmeshMovement.Stop();
        }
    }
}
