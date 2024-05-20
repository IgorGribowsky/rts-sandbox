using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UnitController : MonoBehaviour
{
    
    public List<GameObject> SelectedUnits = new List<GameObject>();

    public Vector3 StartSelectionPoint { get; set; }

    public int SelectedUnitsTeam { get; set; }

    private int playerTeamId;

    void Start()
    {
        playerTeamId = gameObject.GetComponent<PlayerTeamMember>().TeamId;
    }

    private void Update()
    {
    }

    public void MoveTo(Vector3 point)
    {
        if (SelectedUnitsTeam != playerTeamId)
        {
            return;
        }

        point.y = 0.5f;

        foreach (var unit in SelectedUnits) 
        {
            unit.GetComponent<Movement>().MoveTo(point);
        }
    }

    public void StartSelection(Vector3 point)
    {
        StartSelectionPoint = point;
    }

    public void EndSelection(Vector3 point, bool addToPrevoiusSelected)
    {
        SelectedUnits.ForEach(unit => unit.GetComponent<Selectable>().SetSelectionState(false));
        var minx = Mathf.Min(StartSelectionPoint.x, point.x);
        var maxx = Mathf.Max(StartSelectionPoint.x, point.x);
        var minz = Mathf.Min(StartSelectionPoint.z, point.z);
        var maxz = Mathf.Max(StartSelectionPoint.z, point.z);
        var start = new Vector3(minx, 0f, minz);
        var end = new Vector3(maxx, 100f, maxz);

        point.y = 100f;
        Bounds bounds = new Bounds();
        bounds.SetMinMax(start, end);

        var selectedUnits = new List<GameObject>();
        var teamId = 0;

        var selectableUntisInArea = GameObject.FindGameObjectsWithTag("HasTag")
            .Where(o => bounds.Intersects(o.GetComponent<Collider>().bounds))
            .Where(o => o.GetComponent<Selectable>() != null)
            .ToList();

        if (selectableUntisInArea.Any())
        {
            var groupedByTeamId = selectableUntisInArea
                .GroupBy(u => u.GetComponent<TeamMember>().TeamId);

            var playerGroup = groupedByTeamId.FirstOrDefault(u => u.Key == playerTeamId);
            if (playerGroup != null)
            {
                selectedUnits = playerGroup.ToList();
                teamId = playerGroup.Key;
            }
            else
            {
                selectedUnits = selectableUntisInArea
                    .OrderByDescending(u => u.GetComponent<UnitValues>().Rang)
                    .Take(1)
                    .ToList();
                teamId = selectedUnits.First().GetComponent<TeamMember>().TeamId;
            }
        }

        if (addToPrevoiusSelected)
        {
            SelectedUnits.AddRange(selectedUnits.Except(SelectedUnits));
        }
        else 
        {
            SelectedUnits = selectedUnits;
        }

        SelectedUnitsTeam = teamId;
        SelectedUnits.ForEach(unit => unit.GetComponent<Selectable>().SetSelectionState(true));
    }
}