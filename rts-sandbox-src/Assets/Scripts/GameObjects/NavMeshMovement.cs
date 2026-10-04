using Assets.Scripts.Infrastructure.Events;
using Assets.Scripts.Infrastructure.Extensions;
using Assets.Scripts.Infrastructure.Helpers;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class NavMeshMovement : MonoBehaviour
{
    public Vector3 Destination { get => _navmeshAgent.destination; }
    public float StoppingDistance { get => _navmeshAgent.stoppingDistance; }

    private UnitValues _unitValues;
    private NavMeshAgent _navmeshAgent;

    // How far past the agent's stoppingDistance a unit that has stopped still
    // counts as arrived at the object it walked to.
    private const float ArrivalTolerance = 0.1f;

    private bool _goToObjectFlag = false;
    private GameObject _destinationObj;
    private Vector3 _currentDestination;
    private float _thisObjSize;
    private float _distance;
    private ObstacleAvoidanceType _normalAvoidance;

    public event MoveActionEndedHandler NavMeshMovementArrive;
    public void OnNavMeshMovementArrive()
    {
        NavMeshMovementArrive?.Invoke(new EventArgs());
    }

    // Start is called before the first frame update
    void Awake()
    {
        _unitValues = gameObject.GetComponent<UnitValues>();

        _navmeshAgent = gameObject.GetComponent<NavMeshAgent>();
        _navmeshAgent.speed = _unitValues.MovementSpeed;

        _thisObjSize = gameObject.GetSize();
        _normalAvoidance = _navmeshAgent.obstacleAvoidanceType;
    }

    /// <summary>
    /// On the gathering route workers walk through each other, as on gold in
    /// WC3 (T-063): the unit stops steering around others. Everyone else still
    /// steers around it, so soldiers do not walk through workers.
    /// </summary>
    public void SetPassThrough(bool on)
    {
        _navmeshAgent.obstacleAvoidanceType = on ? ObstacleAvoidanceType.NoObstacleAvoidance : _normalAvoidance;
    }

    /// <summary>
    /// Walks to a place of its own around the object (ApproachSlots), taking
    /// one if it holds none there yet. False when the object has no free place:
    /// the caller picks another object or falls back to GoToObject.
    /// </summary>
    public bool TryGoToPlaceAt(GameObject target, float distance, int maxUnits = 0)
    {
        var slots = ApproachSlots.Of(target);

        if (!slots.IsHeldBy(gameObject, out var point))
        {
            var ring = target.GetSize() + _thisObjSize + distance;

            if (!slots.TryTake(gameObject, target.GetBoundCenter(), ring, _thisObjSize,
                _navmeshAgent.agentTypeID, maxUnits, out point))
            {
                return false;
            }
        }

        GoToPlace(point);
        return true;
    }

    /// <summary>Gives the place around the object back, if this unit holds one there.</summary>
    public void ReleasePlaceAt(GameObject target)
    {
        if (target == null)
        {
            return;
        }

        var slots = target.GetComponent<ApproachSlots>();
        if (slots != null)
        {
            slots.Release(gameObject);
        }
    }

    /// <summary>
    /// Like GoToObject, but to a point that does not move: NavMeshMovementArrive
    /// comes when the unit gets there.
    /// </summary>
    public void GoToPlace(Vector3 point)
    {
        _goToObjectFlag = true;
        _destinationObj = null;

        _navmeshAgent.avoidancePriority = 90;
        _navmeshAgent.destination = point;
        _currentDestination = point;
    }

    public void Go(Vector3 destination)
    {
        _goToObjectFlag = false;

        var differenceVector = _currentDestination - destination;
        differenceVector.y = 0;

        if (differenceVector != Vector3.zero)
        {
            _navmeshAgent.avoidancePriority = 90;
            _navmeshAgent.destination = destination;
            _currentDestination = destination;
        }
    }


    public void GoToObject(GameObject destinationObj, float distance)
    {
        _goToObjectFlag = true;

        if (destinationObj != _destinationObj)
        {
            _destinationObj = destinationObj;

            var destinationObjSize = _destinationObj.GetSize();

            _distance = destinationObjSize + _thisObjSize + distance;
            AdjustDestination();
        }
    }

    public void Warp(Vector3 destination)
    {
        _goToObjectFlag = false;

        Vector3 correctedTarget = destination;
        Collider hitCollider = Physics.OverlapSphere(destination, 0.01f)
                          .FirstOrDefault(c => c.gameObject.isStatic);
        if (hitCollider)
        {
            var point = hitCollider.ClosestPoint(transform.position);
            point.y = transform.position.y;
            var dir = (transform.position - point).normalized;
            correctedTarget = point + dir * _navmeshAgent.radius;
        }
        _navmeshAgent.Warp(correctedTarget);
    }


    // Update is called once per frame
    void Update()
    {
        if (_goToObjectFlag && _destinationObj != null)
        {
            AdjustDestination();
        }

        if (_goToObjectFlag)
        {
            float epsilon = 0.02f;

            var v1 = gameObject.transform.position;
            v1.y = 0;
            var v2 = _currentDestination;
            v2.y = 0;

            var distance = Vector2.Distance(new Vector2(v1.x, v1.z), new Vector2(v2.x, v2.z));
            bool equal = distance < epsilon;

            // The agent brakes at its stoppingDistance and a neighbour can nudge
            // it a few centimetres off the point: then it stands still short of
            // epsilon and never arrives (T-035). Standing still within reach
            // counts as arrived.
            bool stoppedClose = !_navmeshAgent.pathPending
                && distance <= _navmeshAgent.stoppingDistance + ArrivalTolerance
                && _navmeshAgent.velocity.sqrMagnitude < 0.01f;

            if (equal || stoppedClose)
            {
                _goToObjectFlag = false;
                OnNavMeshMovementArrive();
            }
        }
    }

    private void AdjustDestination()
    {
        if (_destinationObj == null) return;


        Vector3 position = FindBestPosition();

        var differenceVector = _currentDestination - position;
        differenceVector.y = 0;

        if (differenceVector.magnitude > 0.066f)
        {
            _navmeshAgent.destination = position;
            _currentDestination = position;
        }
    }

    private Vector3 FindBestPosition()
    {
        var destinationObjCenter = _destinationObj.GetBoundCenter();

        Vector3 bestPosition = destinationObjCenter + (gameObject.transform.position - destinationObjCenter).normalized * _distance;

        // Проверка, свободна ли точка
        if (IsPositionFree(bestPosition))
        {
            return bestPosition;
        }

        // Ищем ближайшую свободную точку вокруг цели
        return FindNearestFreePosition(destinationObjCenter, _distance);
    }

    private Vector3 FindNearestFreePosition(Vector3 targetPosition, float radius)
    {
        const int searchSteps = 12;
        for (int i = 0; i < searchSteps; i++)
        {
            float angle = (360f / searchSteps) * i;
            Vector3 offset = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), 0, Mathf.Sin(angle * Mathf.Deg2Rad)) * radius;
            Vector3 candidatePosition = targetPosition + offset;

            if (IsPositionFree(candidatePosition))
            {
                return candidatePosition;
            }
        }
        return targetPosition; // Если ничего не найдено, идем к центру
    }

    private bool IsPositionFree(Vector3 position)
    {
        Collider[] colliders = Physics.OverlapSphere(position, _thisObjSize);
        foreach (var col in colliders)
        {
            if (col.gameObject != gameObject && (col.GetComponent<NavMeshObstacle>() || col.GetComponent<NavMeshAgent>()))
            {
                return false;
            }
        }
        return true;
    }

    public void Stop()
    {
        _goToObjectFlag = false;

        _navmeshAgent.avoidancePriority = 50;
        _navmeshAgent.destination = gameObject.transform.position;
        _currentDestination = gameObject.transform.position;
    }
}
