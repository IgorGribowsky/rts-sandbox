using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEngine;

public class UnitController : MonoBehaviour
{
    public float ClosenessMultiplier = 2f;
    public List<GameObject> SelectedUnits = new List<GameObject>();

    public Vector3 StartSelectionPoint { get; set; }

    public int SelectedUnitsTeamId { get; set; }

    private Dictionary<int, UnitMovementMask> SelectedUnitsMovementMask = new Dictionary<int, UnitMovementMask>();
    private TeamController _teamController;

    private int playerTeamId;

    void Start()
    {
        playerTeamId = gameObject.GetComponent<PlayerTeamMember>().TeamId;

        _teamController = GameObject.FindGameObjectWithTag("GameController")
            .GetComponent<TeamController>();
    }

    private void Update()
    {
    }

    public void OnGroundRightClick(Vector3 point)
    {
        if (SelectedUnitsTeamId != playerTeamId)
        {
            return;
        }

        point.y = 0.5f;

        foreach (var unit in SelectedUnits)
        {
            var unitMovementMaskVector = SelectedUnitsMovementMask[unit.GetInstanceID()].PositionFromCenter;
            var pointToMove = point + unitMovementMaskVector * ClosenessMultiplier;
            unit.GetComponent<UnitEventManager>().OnMoveCommandReceived(pointToMove);
        }
    }

    public void OnUnitRightClick(GameObject target)
    {
        if (SelectedUnitsTeamId != playerTeamId)
        {
            return;
        }

        var targetTeamId = target.GetComponent<TeamMember>().TeamId;
        var allyTeamIds = _teamController.GetAllyTeams(playerTeamId);
        if (playerTeamId == targetTeamId || allyTeamIds.Contains(targetTeamId))
        {
            foreach (var unit in SelectedUnits)
            {
                unit.GetComponent<UnitEventManager>().OnFollowCommandReceived(target);
            }
        }
        else
        {
            foreach (var unit in SelectedUnits)
            {
                unit.GetComponent<UnitEventManager>().OnAttackCommandReceived(target);
                unit.GetComponent<UnitEventManager>().OnFollowCommandReceived(target);
            }
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
            .OrderByDescending(u => u.GetComponent<UnitValues>().Rang)
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

        SelectedUnitsTeamId = teamId;
        SelectedUnits.ForEach(unit => unit.GetComponent<Selectable>().SetSelectionState(true));

        CreateMovememtMask();
    }

    private void CreateMovememtMask()
    {
        SelectedUnitsMovementMask = new Dictionary<int, UnitMovementMask>();
        var unitsInserted = 0;
        var arrayRightSize = 1;
        var arrayDownSize = 1;
        var insertDirectionIsRight = false;

        foreach (var unit in SelectedUnits)
        {
            int insertIndexX;
            int insertIndexY;

            if (insertDirectionIsRight)
            {
                insertIndexX = arrayRightSize - 1;
                insertIndexY = unitsInserted - arrayDownSize * (arrayRightSize - 1);
            }
            else
            {
                insertIndexY = arrayDownSize - 1;
                insertIndexX = unitsInserted - arrayRightSize * (arrayDownSize - 1);
            }

            var unitId = unit.GetInstanceID();
            var value = new UnitMovementMask
            {
                UnitId = unitId,
                PositionX = insertIndexX,
                PositionY = insertIndexY,
                PositionFromCenter = Vector3.zero,
            };

            SelectedUnitsMovementMask.Add(unitId, value);
            unitsInserted++;

            if (unitsInserted == SelectedUnits.Count)
            {
                break;
            }

            if (unitsInserted == arrayRightSize * arrayDownSize)
            {
                if (insertDirectionIsRight)
                {
                    arrayDownSize++;
                }
                else
                {
                    arrayRightSize++;
                }
                insertDirectionIsRight = !insertDirectionIsRight;
            }
        }

        var centerPoint = (arrayRightSize / 2.0f, arrayDownSize / 2.0f);

        foreach (var unit in SelectedUnits)
        {
            var value = SelectedUnitsMovementMask[unit.GetInstanceID()];

            value.PositionFromCenter = new Vector3(value.PositionX - centerPoint.Item1 + 0.5f, 0, value.PositionY - centerPoint.Item2 + 0.5f);

        }
    }

    public class UnitMovementMask
    {
        public float UnitId { get; set; }

        public float PositionX { get; set; }

        public float PositionY { get; set; }

        public Vector3 PositionFromCenter { get; set; }
    }
}