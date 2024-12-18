using Assets.Scripts.Infrastructure.Events;
using System;
using UnityEngine;
using UnityEngine.AI;

public class Movement : MonoBehaviour
{
    private NavMeshMovement _navmeshMovement;
    private UnitEventManager _unitEventManager;

    private bool isProcessing = false;

    void Awake()
    {
        _navmeshMovement = gameObject.GetComponent<NavMeshMovement>();

        _unitEventManager = GetComponent<UnitEventManager>();

        _unitEventManager.MoveActionStarted += MoveTo;
        _unitEventManager.FollowActionStarted += Stop;
        _unitEventManager.AttackActionStarted += Stop;
    }

    protected void MoveTo(MoveActionStartedEventArgs args)
    {
        isProcessing = true;
        _navmeshMovement.Go(args.MovePoint);
    }

    protected void Stop(EventArgs args)
    {
        if (isProcessing)
        {
            _navmeshMovement.Stop();
            isProcessing = false;
        }
    }

    void Update()
    {
        if (isProcessing)
        {
            var differenceVector = _navmeshMovement.Destination - transform.position;
            differenceVector.y = 0;
            if (differenceVector.magnitude <= _navmeshMovement.StoppingDistance)
            {
                Stop(new EventArgs());
                _unitEventManager.OnMoveActionEnded();
            }
        }
    }
}
