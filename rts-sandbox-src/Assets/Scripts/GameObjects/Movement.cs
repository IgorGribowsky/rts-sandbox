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

        _unitEventManager.MoveCommandReceived += MoveTo;
    }

    public void MoveTo(MoveCommandReceivedEventArgs args)
    {
        _navmeshAgent.avoidancePriority = 90;
        _navmeshAgent.destination = args.MovePoint;
    }

    public void SetSpeed(float speed)
    {
        movementSpeed = speed;
        _navmeshAgent.speed = speed;
    }

    void Update()
    {
        if (!_navmeshAgent.pathPending)
        {
            if (_navmeshAgent.remainingDistance <= _navmeshAgent.stoppingDistance)
            {
                if (!_navmeshAgent.hasPath || _navmeshAgent.velocity.sqrMagnitude == 0f)
                {
                    _navmeshAgent.avoidancePriority = 50;
                }
            }
        }
    }
}
