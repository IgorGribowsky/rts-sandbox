using Assets.Scripts.Infrastructure.Events;
using System;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;

public class Movement : MonoBehaviour
{
    private NavMeshAgent _navmeshAgent;
    private UnitEventManager _unitEventManager;
    private UnitValues _unitValues;

    private bool isProcessing = false;

    void Start()
    {
        _unitValues = gameObject.GetComponent<UnitValues>();
        _navmeshAgent = gameObject.GetComponent<NavMeshAgent>();

        _navmeshAgent.speed = _unitValues.MovementSpeed;

        _unitEventManager = GetComponent<UnitEventManager>();

        _unitEventManager.MoveActionStarted += MoveTo;
        _unitEventManager.FollowActionStarted += Stop;
        _unitEventManager.AttackActionStarted += Stop;
    }

    protected void MoveTo(MoveActionStartedEventArgs args)
    {
        isProcessing = true;
        _navmeshAgent.avoidancePriority = 90;
        _navmeshAgent.destination = args.MovePoint;
    }

    protected void Stop(EventArgs args)
    {
        if (isProcessing)
        {
            _navmeshAgent.avoidancePriority = 50;
            isProcessing = false;
        }
    }

    void Update()
    {
        if (isProcessing && _navmeshAgent.remainingDistance <= _navmeshAgent.stoppingDistance)
        {
            Stop(new EventArgs());
            _unitEventManager.OnMoveActionEnded();
        }
    }
}
