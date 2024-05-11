using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class UnitController : MonoBehaviour
{
    public List<GameObject> SelectedUnits;

    public Vector3 StartSelectionPoint { get; set; }

    void Start()
    {
    }

    private void Update()
    {
    }

    public void MoveTo(Vector3 point)
    {
        point.y = 0.5f;

        foreach (var unit in SelectedUnits) 
        {
            var navmeshAgent = unit.GetComponent<NavMeshAgent>();
            navmeshAgent.destination = point;
        }
    }

    public void StartSelection(Vector3 point)
    {
        StartSelectionPoint = point;
    }

    public void EndSelection(Vector3 point, bool addToPrevoiusSelected)
    {
        var minx = Mathf.Min(StartSelectionPoint.x, point.x);
        var maxx = Mathf.Max(StartSelectionPoint.x, point.x);
        var minz = Mathf.Min(StartSelectionPoint.z, point.z);
        var maxz = Mathf.Max(StartSelectionPoint.z, point.z);
        var start = new Vector3(minx, 0f, minz);
        var end = new Vector3(maxx, 100f, maxz);

        point.y = 100f;
        Bounds bounds = new Bounds();
        bounds.SetMinMax(start, end);

        var selectedUnits = GameObject.FindGameObjectsWithTag("HasTag")
            .Where(o => o.GetComponent<Tags>().IsSelectable)
            .Where(o => bounds.Intersects(o.GetComponent<Collider>().bounds))
            .ToList();

        if (addToPrevoiusSelected)
        {
            SelectedUnits.AddRange(selectedUnits.Except(SelectedUnits));
        }
        else 
        {
            SelectedUnits = selectedUnits;
        }
    }
}