using Assets.Scripts.Infrastructure.Constants;
using Assets.Scripts.Infrastructure.Enums;
using Assets.Scripts.Infrastructure.Events;
using Assets.Scripts.Infrastructure.Extensions;
using Assets.Scripts.Infrastructure.Helpers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UnitsController : MonoBehaviour
{
    public float ClosenessMultiplier = 2.5f;
    public List<GameObject> SelectedUnits = new List<GameObject>();

    public Vector3 StartSelectionPoint { get; set; }

    public int SelectedUnitsTeamId { get; set; }

    private Dictionary<int, UnitMovementMask> SelectedUnitsMovementMask = new Dictionary<int, UnitMovementMask>();
    private TeamController _teamController;
    private GameController _gameController;
    private BuildingController _buildingController;
    private BuildingGridController _buildingGridController;
    private PlayerResources _playerResources;
    private PlayerEventController _playerEventController;

    private int playerTeamId;
    private GameObject _unitUnderCursor;

    void Start()
    {
        playerTeamId = gameObject.GetComponent<PlayerTeamMember>().TeamId;

        var gameController = GameObject.FindGameObjectWithTag(Tag.GameController.ToString());
        _teamController = gameController.GetComponent<TeamController>();
        _gameController = gameController.GetComponent<GameController>();
        _buildingController = GetComponent<BuildingController>();
        _buildingGridController = GetComponent<BuildingGridController>();
        _playerResources = GetComponent<PlayerResources>();
        _playerEventController = GetComponent<PlayerEventController>();

        _playerEventController.SelectedUnitDied += SelectedUnitDiedHandler;
        _playerEventController.CursorMoved += CursorMovedHandler;
    }

    private void Update()
    {
    }

    public void RightClickOnResource(GameObject resource, Vector3 point, bool addToCommandsQueue = false)
    {
        if (SelectedUnitsTeamId != playerTeamId)
        {
            return;
        }

        foreach (var unit in SelectedUnits)
        {
            if (unit.GetComponent<UnitValues>().IsHarvestor
                && resource.tag == Tag.HarvestedResource.ToString()
                && unit.GetComponent<UnitValues>().ResourcesCanBeHarvested.Contains(resource.GetComponent<ResourceValues>().ResourceName))
            {
                unit.GetComponent<UnitEventManager>().OnHarvestingCommandReceived(resource, null, false, addToCommandsQueue);
                continue;
            }
            else
            {
                unit.GetComponent<UnitEventManager>().OnMoveCommandReceived(point, addToCommandsQueue);
            }
        }
    }

    public void Build(Vector3 point, bool addToCommandsQueue = false)
    {
        if (SelectedUnitsTeamId != playerTeamId)
        {
            return;
        }

        if (CheckBuilderSelected(out var firstUnitBuilder))
        {
            var buildingValues = _buildingController.Building.GetComponent<BuildingValues>();
            var buildingSize = buildingValues.GridSize;

            if (buildingValues.IsHeldMine && !_buildingGridController.CheckIfMineUnderCursor())
            {
                Debug.Log("Can't build here!");
                return;
            }

            Vector3 resultPoint = buildingValues.IsHeldMine 
                ? _unitUnderCursor.transform.position 
                : point.GetGridPoint(buildingSize);

            var mineToHeld = buildingValues.IsHeldMine ? _unitUnderCursor : null;
            mineToHeld = mineToHeld != null && mineToHeld.GetComponent<BuildingValues>().IsMine ? mineToHeld : null;

            if (addToCommandsQueue)
            {
                firstUnitBuilder.GetComponent<UnitEventManager>().OnBuildCommandReceived(resultPoint, _buildingController.Building, buildingValues.IsHeldMine, mineToHeld, addToCommandsQueue);
            }
            else
            {
                var unitId = firstUnitBuilder.GetComponent<UnitValues>().Id;
                var allBuilders = SelectedUnits.Where(x => x.GetComponent<UnitValues>().Id == unitId);
                GameObject unitToBuild = null;
                foreach (var builder in allBuilders)
                {
                    var isNotActive = !builder.GetComponent<BuildingBehaviour>()?.IsActive;
                    var isReady = isNotActive ?? false;
                    if (isReady)
                    {
                        unitToBuild = builder;
                        break;
                    }
                }

                if (unitToBuild == null)
                {
                    unitToBuild = firstUnitBuilder;
                }

                if (!_buildingGridController.CheckIfCanBuildAt(resultPoint, buildingSize, unitToBuild) && !buildingValues.IsHeldMine)
                {
                    Debug.Log("Can't build here!");
                    return;
                }

                var unitValues = _buildingController.Building.GetComponent<UnitValues>();
                var resourceCost = unitValues.ResourceCost.ToArray();
                if (!_playerResources.CheckIfCanSpendResources(resourceCost))
                {
                    Debug.Log("Not enough resources!");
                    return;
                }
                else if (!_playerResources.CheckIfHaveSupply(resourceCost))
                {
                    Debug.Log("Not enough supply!");
                    return;
                }
                unitToBuild.GetComponent<UnitEventManager>().OnBuildCommandReceived(resultPoint, _buildingController.Building, buildingValues.IsHeldMine, mineToHeld, addToCommandsQueue);
                _buildingController.DisableBuildingMod();
            }
        }
    }


    public bool CheckBuilderSelected()
    {
        if (SelectedUnitsTeamId != playerTeamId)
        {
            return false;
        }

        return SelectedUnits.FirstOrDefault()?.GetComponent<UnitValues>()?.IsBuilder ?? false;
    }

    public bool CheckBuilderSelected(out GameObject builder)
    {
        if (SelectedUnitsTeamId != playerTeamId)
        {
            builder = null;
            return false;
        }

        builder = SelectedUnits.FirstOrDefault();
        var response = builder?.GetComponent<UnitValues>()?.IsBuilder ?? false;
        return response;
    }

    public void OnHoldKeyDown(bool addToCommandsQueue = false)
    {
        foreach (var unit in SelectedUnits)
        {
            unit.GetComponent<UnitEventManager>().OnHoldCommandReceived(addToCommandsQueue);
        }
    }

    public void OnGroundRightClick(Vector3 point, bool addToCommandsQueue = false)
    {
        if (SelectedUnitsTeamId != playerTeamId)
        {
            return;
        }

        point.y = 0.5f;

        var movableSelectedUnits = GetMovableSelectedUnits();
        foreach (var unit in movableSelectedUnits)
        {
            var unitMovementMaskVector = SelectedUnitsMovementMask[unit.GetInstanceID()].PositionFromCenter;
            var pointToMove = point + unitMovementMaskVector * ClosenessMultiplier;
            unit.GetComponent<UnitEventManager>().OnMoveCommandReceived(pointToMove, addToCommandsQueue);
        }
    }

    public void OnGroundAClick(Vector3 point, bool addToCommandsQueue = false)
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
            unit.GetComponent<UnitEventManager>().OnAMoveCommandReceived(pointToMove, addToCommandsQueue);
        }
    }

    public void OnUnitAClick(GameObject target, bool addToCommandsQueue = false)
    {
        if (SelectedUnitsTeamId != playerTeamId)
        {
            return;
        }

        if (!_gameController.FriendlyFire)
        {
            OnUnitRightClick(target, addToCommandsQueue);
            return;
        }

        foreach (var unit in SelectedUnits)
        {
            if (target == unit)
            {
                continue;
            }

            var damageType = unit.GetComponent<UnitValues>().DamageType;

            if (!target.CanBeAttacked(damageType))
            {
                Debug.Log("Unit can't be attacked!");
                continue;
            }

            unit.GetComponent<UnitEventManager>().OnAttackCommandReceived(target, addToCommandsQueue);
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

        if (!SelectedUnits.Any())
        {
            return;
        }

        var targetTeamId = target.GetComponent<TeamMember>().TeamId;
        var allyTeamIds = _teamController.GetAllyTeams(playerTeamId);

        if (allyTeamIds.Contains(targetTeamId))
        {
            foreach (var unit in SelectedUnits)
            {
                if (target == unit)
                {
                    unit.GetComponent<UnitEventManager>().OnMoveCommandReceived(target.transform.position, addToCommandsQueue);
                    continue;
                }

                if (targetTeamId == playerTeamId 
                    && unit.GetComponent<UnitValues>().IsMiner
                    && target.GetComponent<UnitValues>().IsBuilding
                    && target.GetComponent<BuildingValues>().IsHeldMine)
                {
                    unit.GetComponent<UnitEventManager>().OnMineCommandReceived(target, addToCommandsQueue);
                    continue;
                }

                if (targetTeamId == playerTeamId
                    && unit.GetComponent<UnitValues>().IsHarvestor
                    && unit.GetComponent<HarvestingBehaviour>().CurrentResourceValues > 0
                    && unit.GetComponent<HarvestingBehaviour>().CurrentResource != null
                    && target.GetComponent<HarvestedResourcesStorage>() != null
                    && target.GetComponent<HarvestedResourcesStorage>().isActiveAndEnabled
                    && target.GetComponent<HarvestedResourcesStorage>().StoredResources.Contains(unit.GetComponent<HarvestingBehaviour>().CurrentResource.Value))
                {
                    unit.GetComponent<UnitEventManager>().OnHarvestingCommandReceived(null, target, true, addToCommandsQueue);
                    continue;
                }

                unit.GetComponent<UnitEventManager>().OnFollowCommandReceived(target, addToCommandsQueue);
            }
        }
        else
        {
            foreach (var unit in SelectedUnits)
            {
                var damageType = unit.GetComponent<UnitValues>().DamageType;

                if (!target.CanBeAttacked(damageType))
                {
                    Debug.Log("Unit can't be attacked!");
                    continue;
                }

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

    public void EndSelection(Vector3 point, bool addToPreviousSelection)
    {
        var firstUnit = SelectedUnits.FirstOrDefault();

        SelectedUnits.ForEach(unit => unit.GetComponent<Selectable>().SetSelectionState(false));

        var bounds = CreateBoundsFromSelection(StartSelectionPoint, point);

        var selectableUnits = GameObject.FindGameObjectsWithTag(Tag.Unit.ToString())
            .Where(o => bounds.Intersects(o.GetComponent<Collider>().bounds))
            .Where(o => o.GetComponent<Selectable>() != null)
            .OrderByDescending(u => u.GetComponent<UnitValues>().Rang)
            .ToList();

        var selectedUnits = new List<GameObject>();
        var teamId = 0;

        if (selectableUnits.Any())
        {
            var groupedByTeamId = selectableUnits.GroupBy(u => u.GetComponent<TeamMember>().TeamId);
            var playerGroup = groupedByTeamId.FirstOrDefault(g => g.Key == playerTeamId);

            if (playerGroup != null)
            {
                teamId = playerGroup.Key;

                //units without buldings
                selectedUnits = playerGroup
                    .GroupBy(u => u.GetComponent<UnitValues>().IsBuilding)
                    .OrderBy(u => u.Key)
                    .First()
                    .ToList();
            }
            else
            {
                selectedUnits = selectableUnits.Take(1).ToList();
                teamId = selectedUnits.First().GetComponent<TeamMember>().TeamId;
                addToPreviousSelection = false;
            }
        }
        addToPreviousSelection = CanAddToPreviousSelection(addToPreviousSelection, teamId);

        ApplySelection(selectedUnits, addToPreviousSelection);
        PostSelectActions(firstUnit, teamId);
    }

    public void OnDoubleClick(GameObject targetUnit, bool addToPreviousSelection)
    {
        var targetTeamId = targetUnit.GetComponent<TeamMember>().TeamId;
        var unitId = targetUnit.GetComponent<UnitValues>().Id;

        if (targetTeamId != playerTeamId)
            return;

        addToPreviousSelection = CanAddToPreviousSelection(addToPreviousSelection, targetTeamId);

        var firstUnit = SelectedUnits.FirstOrDefault();

        var unitsToSelect = targetUnit.GetAllUnitsInRadius(GameConstants.DoubleClickSelectDistance, unit =>
        {
            var teamMember = unit.GetComponent<TeamMember>();
            var unitValues = unit.GetComponent<UnitValues>();
            return teamMember != null && unitValues != null
                && teamMember.TeamId == targetTeamId
                && unitValues.Id == unitId;
        }).ToList();

        ApplySelection(unitsToSelect, addToPreviousSelection);
        PostSelectActions(firstUnit, targetTeamId);
    }

    public void OnCancelClick()
    {
        if (SelectedUnitsTeamId != playerTeamId)
        {
            return;
        }
        var selectedUnitsArray = SelectedUnits.ToArray();
        for (var i = 0; i < selectedUnitsArray.Length; i++)
        {
            var unit = selectedUnitsArray[i];
            unit.GetComponent<UnitEventManager>().OnCanceled(unit);
        }
    }

    private void ApplySelection(List<GameObject> units, bool addToPreviousSelection)
    {
        if (addToPreviousSelection)
        {
            SelectedUnits.AddRange(units.Except(SelectedUnits));
        }
        else
        {
            SelectedUnits = units;
        }
    }

    private bool CanAddToPreviousSelection(bool requestedAdd, int targetTeamId)
    {
        return requestedAdd && SelectedUnitsTeamId == playerTeamId && targetTeamId == playerTeamId;
    }

    private void PostSelectActions(GameObject firstUnit, int teamId)
    {
        SelectedUnitsTeamId = teamId;
        SelectedUnits.ForEach(unit => unit.GetComponent<Selectable>().SetSelectionState(true));

        CreateMovememtMask();

        if (_buildingController.BuildingMenuMod)
        {
            if (firstUnit != SelectedUnits.FirstOrDefault())
            {
                _buildingController.DisableBuildingMenuMod();
            }
        }
    }

    private Bounds CreateBoundsFromSelection(Vector3 start, Vector3 end)
    {
        var minx = Mathf.Min(start.x, end.x);
        var maxx = Mathf.Max(start.x, end.x);
        var minz = Mathf.Min(start.z, end.z);
        var maxz = Mathf.Max(start.z, end.z);

        var min = new Vector3(minx, 0f, minz);
        var max = new Vector3(maxx, 100f, maxz);

        Bounds bounds = new Bounds();
        bounds.SetMinMax(min, max);
        return bounds;
    }

    private void CreateMovememtMask()
    {
        SelectedUnitsMovementMask = new Dictionary<int, UnitMovementMask>();
        var unitsInserted = 0;
        var arrayRightSize = 1;
        var arrayDownSize = 1;
        var insertDirectionIsRight = false;

        var movableSelectedUnits = GetMovableSelectedUnits();

        foreach (var unit in movableSelectedUnits)
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

            if (unitsInserted == movableSelectedUnits.Count)
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

        foreach (var unit in movableSelectedUnits)
        {
            var value = SelectedUnitsMovementMask[unit.GetInstanceID()];

            value.PositionFromCenter = new Vector3(value.PositionX - centerPoint.Item1 + 0.5f, 0, value.PositionY - centerPoint.Item2 + 0.5f);
        }
    }

    protected void CursorMovedHandler(CursorMovedEventArgs args)
    {
        _unitUnderCursor = args.UnitUnderCursor;
    }

    private List<GameObject> GetMovableSelectedUnits()
    {
        return SelectedUnits
            .Where(x => x.GetComponent<MovementBehaviour>() != null)
            .ToList();
    }

    private class UnitMovementMask
    {
        public float UnitId { get; set; }

        public float PositionX { get; set; }

        public float PositionY { get; set; }

        public Vector3 PositionFromCenter { get; set; }
    }

    private void OnDestroy()
    {
        _playerEventController.SelectedUnitDied -= SelectedUnitDiedHandler;
        _playerEventController.CursorMoved -= CursorMovedHandler;
    }
}