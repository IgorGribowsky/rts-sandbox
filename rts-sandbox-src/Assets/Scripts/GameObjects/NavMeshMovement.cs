using Assets.Scripts.Infrastructure.Events;
using Assets.Scripts.Infrastructure.Extensions;
using Assets.Scripts.Infrastructure.Helpers;
using System;
using UnityEngine;
using UnityEngine.AI;

public class NavMeshMovement : MonoBehaviour
{
    public Vector3 Destination { get => _navmeshAgent.destination; }
    public float StoppingDistance { get => _navmeshAgent.stoppingDistance; }

    private UnitValues _unitValues;
    private NavMeshAgent _navmeshAgent;

    private bool _goToObjectFlag = false;
    private GameObject _destinationObj;
    private float _thisObjSize;
    private float _distance;

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
    }

    public void Go(Vector3 destination)
    {
        _goToObjectFlag = false;

        var differenceVector = _navmeshAgent.destination - destination;
        differenceVector.y = 0;

        if (differenceVector != Vector3.zero)
        {
            _navmeshAgent.avoidancePriority = 90;
            _navmeshAgent.destination = destination;
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
            var v2 = _navmeshAgent.destination;
            v2.y = 0;

            bool equal = Vector2.Distance(new Vector2(v1.x, v1.z), new Vector2(v2.x, v2.z)) < epsilon;

            if (equal)
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
        _navmeshAgent.destination = position;
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
    }
}
