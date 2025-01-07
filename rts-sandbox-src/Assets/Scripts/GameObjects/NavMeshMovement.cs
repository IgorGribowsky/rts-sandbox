using Assets.Scripts.Infrastructure.Extensions;
using UnityEngine;
using UnityEngine.AI;

public class NavMeshMovement : MonoBehaviour
{
    public Vector3 Destination { get => _navmeshAgent.destination; }
    public float StoppingDistance { get => _navmeshAgent.stoppingDistance; }

    private UnitValues _unitValues;
    private NavMeshAgent _navmeshAgent;
    // Start is called before the first frame update
    void Awake()
    {
        _unitValues = gameObject.GetComponent<UnitValues>();

        _navmeshAgent = gameObject.GetComponent<NavMeshAgent>();
        _navmeshAgent.speed = _unitValues.MovementSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Go(Vector3 destination)
    {
        if (!_navmeshAgent.destination.IsEqual(destination))
        {
            _navmeshAgent.avoidancePriority = 90;
            _navmeshAgent.destination = destination;
        }
    }

    public void Stop()
    {
        _navmeshAgent.avoidancePriority = 50;
        _navmeshAgent.destination = gameObject.transform.position;
    }
}
