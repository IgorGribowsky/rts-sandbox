using Assets.Scripts.Infrastructure.Events;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UnitController : MonoBehaviour
{
    public float ClosenessMultiplier = 2.5f;
    public List<GameObject> SelectedUnits = new List<GameObject>();

    public Vector3 StartSelectionPoint { get; set; }

    public int SelectedUnitsTeamId { get; set; }

    private Dictionary<int, UnitMovementMask> SelectedUnitsMovementMask = new Dictionary<int, UnitMovementMask>();
    private TeamController _teamController;

    private int playerTeamId;

    public event DiedHandler SelectedUnitDied;

    public void OnSelectedUnitDied(GameObject dead)
    {
        SelectedUnitDied?.Invoke(new DiedEventArgs(null, dead));
    }

    void Start()
    {
        playerTeamId = gameObject.GetComponent<PlayerTeamMember>().TeamId;

        _teamController = GameObject.FindGameObjectWithTag("GameController")
            .GetComponent<TeamController>();

        SelectedUnitDied += SelectedUnitDiedHandler;
    }

    private void Update()
    {
    }

    public void OnGroundRightClick(Vector3 point, bool addToCommandsQueue = false)
    {
        //if (SelectedUnitsTeamId != playerTeamId)
        //{
        //    return;
        //}

        point.y = 0.5f;

        foreach (var unit in SelectedUnits)
        {
            var unitMovementMaskVector = SelectedUnitsMovementMask[unit.GetInstanceID()].PositionFromCenter;
            var pointToMove = point + unitMovementMaskVector * ClosenessMultiplier;
            unit.GetComponent<UnitEventManager>().OnMoveCommandReceived(pointToMove, addToCommandsQueue);
        }
    }

    public Vector3 GetTheMostRangedUnitPosition()
    {
        var unit = SelectedUnits.OrderByDescending(u => u.GetComponent<UnitValues>().Rang).FirstOrDefault();

        return unit?.transform.position ?? Vector3.zero;
    }

    public void OnUnitRightClick(GameObject target, bool addToCommandsQueue = false)
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
                unit.GetComponent<UnitEventManager>().OnFollowCommandReceived(target, addToCommandsQueue);
            }
        }
        else
        {
            foreach (var unit in SelectedUnits)
            {
                unit.GetComponent<UnitEventManager>().OnAttackCommandReceived(target, addToCommandsQueue);
            }
        }
    }

    public void ProduceUnit(int num)
    {
        if (SelectedUnitsTeamId != playerTeamId)
        {
            return;
        }

        var firstUnit = SelectedUnits.FirstOrDefault();

        if (firstUnit == null) 
        {
            return;
        }

        var unitValues = firstUnit.GetComponent<UnitValues>();

        if (!unitValues.CanProduceUnits)
        {
            return;
        }

        var unitToProduce = unitValues.UnitsToProduce.ElementAtOrDefault(num);

        if (unitToProduce == null)
        {
            return;
        }

        var unitId = unitToProduce.GetComponent<UnitValues>().Id;

        var similarProducingUnits = SelectedUnits.Where(u => u.GetComponent<UnitValues>().Id == unitValues.Id);

        foreach (var producingUnit in similarProducingUnits)
        {
            producingUnit.GetComponent<UnitEventManager>().OnProduceCommandReceived(unitId);
        }
    }

    public void SelectedUnitDiedHandler(DiedEventArgs args)
    {
        SelectedUnits.Remove(args.Dead);
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

        var selectableUntisInArea = GameObject.FindGameObjectsWithTag("Unit")
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