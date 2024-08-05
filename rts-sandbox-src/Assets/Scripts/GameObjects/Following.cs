using Assets.Scripts.Infrastructure.Events;
using System;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;

public class Following : MonoBehaviour
{
    private float movementSpeed;

    private NavMeshAgent _navmeshAgent;
    private UnitEventManager _unitEventManager;

    private bool isProcessing = false;
    private GameObject target = null;

    void Start()
    {
        movementSpeed = gameObject.GetComponent<UnitValues>().MovementSpeed;
        _navmeshAgent = gameObject.GetComponent<NavMeshAgent>();

        _navmeshAgent.speed = movementSpeed;

        _unitEventManager = GetComponent<UnitEventManager>();

        _unitEventManager.FollowActionStarted += Follow;
        _unitEventManager.AttackActionStarted += Stop;
        _unitEventManager.MoveActionStarted += Stop;

    }

    protected void Follow(FollowActionStartedEventArgs args)
    {
        _navmeshAgent.isStopped = false;
        _navmeshAgent.avoidancePriority = 90;
        target = args.Target;
        isProcessing = true;
    }

    protected void SetSpeed(float speed)
    {
        movementSpeed = speed;
        _navmeshAgent.speed = speed;
    }

    protected void Stop(EventArgs args)
    {
        target = null;
        _navmeshAgent.isStopped = true;
        _navmeshAgent.avoidancePriority = 50;
        _unitEventManager.OnMoveActionEnded();
        isProcessing = false;
    }

    void Update()
    {
        if (target != null)
        {
            _navmeshAgent.destination = target.transform.position;
        }
        else if (isProcessing)
        {
            Stop(new EventArgs());
        }
    }

}
