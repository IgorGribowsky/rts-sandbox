using Assets.Scripts.Infrastructure.Constants;
using Assets.Scripts.Infrastructure.Events;
using System;
using UnityEngine;
using UnityEngine.AI;

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
        target = args.Target;
        _navmeshAgent.avoidancePriority = 90;
        isProcessing = true;
    }

    protected void Stop(EventArgs args)
    {
        if (isProcessing)
        {
            target = null;
            _navmeshAgent.avoidancePriority = 50;
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
                    _navmeshAgent.destination = target.transform.position;
                }
                else
                {
                    _navmeshAgent.destination = transform.position;
                }
            }
            else
            {
                Stop(new EventArgs());
                _unitEventManager.OnFollowActionEnded();
            }
        }

    }

}
