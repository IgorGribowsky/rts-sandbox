using UnityEngine;
using UnityEngine.AI;

public class UnitController : MonoBehaviour
{
    public GameObject SelectedUnit;

    private NavMeshAgent _navmeshAgent;
    private Transform _tramsform;

    void Start()
    {
        _navmeshAgent = SelectedUnit.GetComponent<NavMeshAgent>();
        _tramsform = SelectedUnit.transform;
    }

    public void MoveTo(Vector3 point)
    {
        point.y = 0.5f;
        Debug.Log(point);

        _navmeshAgent.destination = point;
    }
}
