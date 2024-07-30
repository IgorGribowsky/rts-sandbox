using Assets.Scripts.Infrastructure.Events;
using UnityEngine;
using UnityEngine.AI;

public class Movement : MonoBehaviour
{
    private float movementSpeed;

    private NavMeshAgent _navmeshAgent;
    private UnitEventManager _unitEventManager;

    void Start()
    {
        movementSpeed = gameObject.GetComponent<UnitValues>().MovementSpeed;
        _navmeshAgent = gameObject.GetComponent<NavMeshAgent>();

        _navmeshAgent.speed = movementSpeed;

        _unitEventManager = GetComponent<UnitEventManager>();

        _unitEventManager.MoveActionStarted += MoveTo;
    }

    protected void MoveTo(MoveActionStartedEventArgs args)
    {
        _navmeshAgent.isStopped = false;
        _navmeshAgent.avoidancePriority = 90;
        _navmeshAgent.destination = args.MovePoint;
    }

    protected void SetSpeed(float speed)
    {
        movementSpeed = speed;
        _navmeshAgent.speed = speed;
    }

    void Update()
    {
        if (!_navmeshAgent.isStopped
            && _navmeshAgent.remainingDistance <= _navmeshAgent.stoppingDistance)
        {
            _navmeshAgent.isStopped = true;
            _navmeshAgent.avoidancePriority = 50;
            _unitEventManager.OnMoveActionEnded();
        }
    }
}
