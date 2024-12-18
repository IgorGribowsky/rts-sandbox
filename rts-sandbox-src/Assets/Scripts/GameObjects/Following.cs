using Assets.Scripts.Infrastructure.Constants;
using Assets.Scripts.Infrastructure.Events;
using System;
using UnityEngine;
using UnityEngine.AI;

public class Following : MonoBehaviour
{
    private NavMeshMovement _navmeshMovement;
    private UnitEventManager _unitEventManager;

    private bool isProcessing = false;
    private GameObject target = null;

    void Start()
    {
        _navmeshMovement = gameObject.GetComponent<NavMeshMovement>();

        _unitEventManager = GetComponent<UnitEventManager>();

        _unitEventManager.FollowActionStarted += Follow;
        _unitEventManager.AttackActionStarted += Stop;
        _unitEventManager.MoveActionStarted += Stop;
    }

    protected void Follow(FollowActionStartedEventArgs args)
    {
        if (args.Target == null)
        {
            _unitEventManager.OnFollowActionEnded();
            return;
        }

        target = args.Target;
        isProcessing = true;
    }

    protected void Stop(EventArgs args)
    {
        if (isProcessing)
        {
            target = null;
            isProcessing = false;
        }
    }

    void Update()
    {
        if (isProcessing) 
        {
            if (target != null)
            {
                var distanceToTarget = (transform.position - target.transform.position).magnitude;
                if (distanceToTarget > Constants.FollowingDistance)
                {
                    _navmeshMovement.Go(target.transform.position);
                }
                else
                {
                    _navmeshMovement.Stop();
                }
            }
            else
            {
                Stop(new EventArgs());
                _navmeshMovement.Stop();
                _unitEventManager.OnFollowActionEnded();
            }
        }

    }

}
