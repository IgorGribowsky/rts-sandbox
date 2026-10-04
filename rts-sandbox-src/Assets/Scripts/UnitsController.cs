using Assets.Scripts.GameObjects;
using Assets.Scripts.GameObjects.UnitBehaviour;
using Assets.Scripts.Infrastructure.Constants;
using Assets.Scripts.Infrastructure.Enums;
using Assets.Scripts.Infrastructure.Events;
using Assets.Scripts.Infrastructure.Extensions;
using Assets.Scripts.Infrastructure.Helpers;
using RtsSandbox.Rules;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UnitsController : MonoBehaviour
{
    public float ClosenessMultiplier = 2.5f;

    /// <summary>
    /// The selection, alive units only (T-041). A destroyed object is dropped
    /// here, at the one entry point, so no caller has to remember to check:
    /// the died event normally removes a unit first, this is the safety net
    /// for whatever slips past it.
    /// </summary>
    public List<GameObject> SelectedUnits
    {
        get
        {
            if (_selectedUnits.RemoveAll(unit => unit == null) > 0)
            {
                _selectionPruned = true;
            }

            return _selectedUnits;
        }
    }

    private List<GameObject> _selectedUnits = new List<GameObject>();

    // Set by the getter, raised in LateUpdate: telling the HUD from inside a
    // getter would rebuild it in the middle of whatever loop asked.
    private bool _selectionPruned;

    public Vector3 StartSelectionPoint { get; set; }

    public int SelectedUnitsTeamId { get; set; }

    /// <summary>
    /// The unit the HUD shows and the skill keys cast with: the first of the
    /// selection, which is the highest Rang when picked with a frame (M-022).
    /// </summary>
    public GameObject MainSelectedUnit => SelectedUnits.FirstOrDefault();

    private TeamController _teamController;
    private GameController _gameController;
    private BuildingController _buildingController;
    private BuildingGridController _buildingGridController;
    private PlayerResources _playerResources;
    private PlayerEventController _playerEventController;
    private FogOfWar _fogOfWar;

    private int playerTeamId;
    private GameObject _unitUnderCursor;

    void Awake()
    {
        playerTeamId = gameObject.GetComponent<PlayerTeamMember>().TeamId;

        var gameController = GameObject.FindGameObjectWithTag(Tag.GameController.ToString());
        _teamController = gameController.GetComponent<TeamController>();
        _gameController = gameController.GetComponent<GameController>();
        _buildingController = GetComponent<BuildingController>();
        _buildingGridController = GetComponent<BuildingGridController>();
        _playerResources = GetComponent<PlayerResources>();
        _playerEventController = GetComponent<PlayerEventController>();
        _fogOfWar = gameController.GetComponent<FogOfWar>();
    }

    private void OnEnable()
    {
        if (_playerEventController == null)
        {
            return;
        }

        _playerEventController.SelectedUnitDied += SelectedUnitDiedHandler;
        _playerEventController.CursorMoved += CursorMovedHandler;

        if (_fogOfWar != null)
        {
            _fogOfWar.SightsChanged += FogSightsChangedHandler;
        }
    }

    private void OnDisable()
    {
        if (_playerEventController == null)
        {
            return;
        }

        _playerEventController.SelectedUnitDied -= SelectedUnitDiedHandler;
        _playerEventController.CursorMoved -= CursorMovedHandler;

        if (_fogOfWar != null)
        {
            _fogOfWar.SightsChanged -= FogSightsChangedHandler;
        }
    }

    public void RightClickOnResource(GameObject resource, Vector3 point, bool addToCommandsQueue = false)
    {
        if (SelectedUnitsTeamId != playerTeamId)
        {
            return;
        }

        foreach (var unit in SelectedUnits)
        {
            var harvestingValues = unit.GetComponent<HarvestingValues>();
            if (harvestingValues != null
                && harvestingValues.IsHarvestor
                && resource.tag == Tag.HarvestedResource.ToString()
                && harvestingValues.HarvestableResources.Contains(resource.GetComponent<ResourceValues>().Resource))
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
            var buildingType = _buildingController.Building;
            var buildingSize = buildingType.Building.GridSize;

            if (buildingType.IsHeldMine && !_buildingGridController.CheckIfMineUnderCursor())
            {
                Debug.Log("Can't build here!");
                return;
            }

            Vector3 resultPoint = buildingType.IsHeldMine 
                ? _unitUnderCursor.transform.position 
                : point.GetGridPoint(buildingSize);

            var mineToHeld = buildingType.IsHeldMine
                && _unitUnderCursor?.GetComponent<BuildingValues>()?.IsMine == true
                    ? _unitUnderCursor
                    : null;

            if (addToCommandsQueue)
            {
                firstUnitBuilder.GetComponent<UnitEventManager>().OnBuildCommandReceived(resultPoint, buildingType, buildingType.IsHeldMine, mineToHeld, addToCommandsQueue);
            }
            else
            {
                var unitId = firstUnitBuilder.GetComponent<UnitValues>().Id;
                var allBuilders = SelectedUnits.Where(x => x.GetComponent<UnitValues>().Id == unitId);
                GameObject unitToBuild = null;
                foreach (var builder in allBuilders)
                {
                    var isNotActive = !builder.GetComponent<UnitBehaviourManager>()?.IsBehaviourActive<BuildingBehaviour>();
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

                if (!_buildingGridController.CheckIfCanBuildAt(resultPoint, buildingSize, unitToBuild) && !buildingType.IsHeldMine)
                {
                    Debug.Log("Can't build here!");
                    return;
                }

                var resourceCost = buildingType.Stats.ResourceCost.ToArray();
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
                unitToBuild.GetComponent<UnitEventManager>().OnBuildCommandReceived(resultPoint, buildingType, buildingType.IsHeldMine, mineToHeld, addToCommandsQueue);
                _buildingController.DisableBuildingMod();
            }
        }
    }

    public bool CheckBuilderSelected() => CheckBuilderSelected(out _);

    public bool CheckBuilderSelected(out GameObject builder)
    {
        if (SelectedUnitsTeamId != playerTeamId)
        {
            builder = null;
            return false;
        }

        builder = SelectedUnits.FirstOrDefault();
        var response = builder?.GetComponent<BuilderValues>()?.IsBuilder ?? false;
        return response;
    }

    /// <summary>"Gather" (M-023): every selected worker of ours goes to work on its own.</summary>
    public void OnGatherKeyDown(bool addToCommandsQueue = false)
    {
        if (SelectedUnitsTeamId != playerTeamId)
        {
            return;
        }

        foreach (var unit in SelectedUnits)
        {
            if (GatherTargets.CanGather(unit))
            {
                unit.GetComponent<UnitEventManager>().OnGatherCommandReceived(addToCommandsQueue);
            }
        }
    }

    /// <summary>`S`: the selection drops its orders and stands (T-032).</summary>
    public void OnStopKeyDown()
    {
        if (SelectedUnitsTeamId != playerTeamId)
        {
            return;
        }

        foreach (var unit in SelectedUnits)
        {
            unit.GetComponent<UnitEventManager>().OnStopCommandReceived();
        }
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

        // Producing buildings in the selection take the click as their rally
        // point (T-030); they never were in the movable list, so the units of a
        // mixed selection still walk.
        foreach (var unit in SelectedUnits)
        {
            var producing = unit.GetComponent<UnitProducing>();
            if (producing != null && producing.enabled)
            {
                producing.SetRallyPoint(point);
            }
        }

        var formation = BuildFormation(point);
        foreach (var unit in GetMovableSelectedUnits())
        {
            var pointToMove = point + GetFormationOffset(formation, unit);
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

        var formation = BuildFormation(point);
        foreach (var unit in SelectedUnits)
        {
            var pointToMove = point + GetFormationOffset(formation, unit);
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
                    && unit.GetComponent<HarvestingValues>()?.IsMiner == true
                    && target.GetComponent<UnitValues>().IsBuilding
                    && target.GetComponent<BuildingValues>().IsHeldMine)
                {
                    unit.GetComponent<UnitEventManager>().OnMineCommandReceived(target, addToCommandsQueue);
                    continue;
                }

                var harvesting = unit.GetComponent<UnitBehaviourManager>()?.Get<HarvestingBehaviour>();

                if (targetTeamId == playerTeamId
                    && unit.GetComponent<HarvestingValues>()?.IsHarvestor == true
                    && harvesting != null
                    && harvesting.CurrentResourceValues > 0
                    && harvesting.CurrentResource != null
                    && target.GetComponent<HarvestedResourcesStorage>() != null
                    && target.GetComponent<HarvestedResourcesStorage>().isActiveAndEnabled
                    && target.GetComponent<HarvestedResourcesStorage>().StoredResources.Contains(harvesting.CurrentResource))
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
        var buildingValues = firstUnit.GetComponent<BuildingValues>();

        if (buildingValues == null || !buildingValues.CanProduceUnits)
        {
            return;
        }

        var unitToProduce = buildingValues.UnitsToProduce.ElementAtOrDefault(num);

        if (unitToProduce == null)
        {
            return;
        }

        var unitId = unitToProduce.Id;

        var similarProducingUnits = SelectedUnits.Where(u => u.GetComponent<UnitValues>().Id == unitValues.Id);

        foreach (var producingUnit in similarProducingUnits)
        {
            producingUnit.GetComponent<UnitEventManager>().OnProduceCommandReceived(unitId);
        }
    }

    /// <summary>
    /// Takes the unit at this place out of the main building's queue, price
    /// back (M-011). Only the main one: its queue is the one on the screen.
    /// </summary>
    public void CancelProduction(int slot)
    {
        if (SelectedUnitsTeamId != playerTeamId)
        {
            return;
        }

        var producing = SelectedUnits.FirstOrDefault()?.GetComponent<UnitProducing>();
        if (producing != null)
        {
            producing.Cancel(slot);
        }
    }

    private void LateUpdate()
    {
        if (_selectionPruned)
        {
            _selectionPruned = false;
            RaiseSelectionChanged();
        }
    }

    public void SelectedUnitDiedHandler(DiedEventArgs args)
    {
        if (SelectedUnits.Remove(args.Dead))
        {
            RaiseSelectionChanged();
        }
    }

    /// <summary>
    /// A selected unit of another team went into the fog of war: it leaves the
    /// selection, as in WC3 (M-027, Q-20). The player's own and allied units
    /// are never hidden, so only someone else's selection can shrink here.
    /// </summary>
    private void FogSightsChangedHandler()
    {
        var removed = false;
        for (var i = _selectedUnits.Count - 1; i >= 0; i--)
        {
            var unit = _selectedUnits[i];
            if (unit == null || FogOfWar.IsSeen(unit))
            {
                continue;
            }

            unit.GetComponent<Selectable>()?.SetSelectionState(false);
            _selectedUnits.RemoveAt(i);
            removed = true;
        }

        if (removed)
        {
            RaiseSelectionChanged();
        }
    }

    private void RaiseSelectionChanged()
    {
        _playerEventController.OnSelectionChanged(SelectedUnits, MainSelectedUnit, SelectedUnitsTeamId);
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
            .Where(o => IsAlive(o))
            .Where(o => FogOfWar.IsSeen(o))
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
                && unitValues.Id == unitId
                && IsAlive(unit);
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
            _selectedUnits = units;
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

        if (_buildingController.BuildingMenuMod)
        {
            if (firstUnit != SelectedUnits.FirstOrDefault())
            {
                _buildingController.DisableBuildingMenuMod();
            }
        }

        RaiseSelectionChanged();
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

    protected void CursorMovedHandler(CursorMovedEventArgs args)
    {
        _unitUnderCursor = args.UnitUnderCursor;
    }

    /// <summary>
    /// Is this unit still alive? Destroy() only marks an object: it dies at the
    /// end of the frame, and until then FindGameObjectsWithTag keeps returning it
    /// with its tag, collider and components in place. Without this check a unit
    /// that died earlier in the same frame gets selected again right after it was
    /// correctly dropped from the selection, and the selection is left holding a
    /// destroyed object for good (T-040).
    /// </summary>
    private static bool IsAlive(GameObject unit)
    {
        var unitValues = unit.GetComponent<UnitValues>();

        return unitValues == null || unitValues.CurrentHp > 0;
    }

    /// <summary>
    /// Where each unit goes for an order to this point (T-034): the places from
    /// FormationRules — melee in front, the higher Rang in front — turned so that
    /// the front faces the way the group goes, as in WC3. Built at the order and
    /// not at the selection, because "front" depends on where the order sends it.
    /// </summary>
    private Dictionary<GameObject, Vector3> BuildFormation(Vector3 point)
    {
        var formation = new Dictionary<GameObject, Vector3>();
        var movable = GetMovableSelectedUnits();

        if (movable.Count == 0)
        {
            return formation;
        }

        var center = Vector3.zero;
        foreach (var unit in movable)
        {
            center += unit.transform.position;
        }

        center /= movable.Count;

        var forward = point - center;
        forward.y = 0f;
        forward = forward.sqrMagnitude > 0.01f ? forward.normalized : Vector3.forward;
        var right = new Vector3(forward.z, 0f, -forward.x);

        var members = new FormationMember[movable.Count];
        for (var i = 0; i < movable.Count; i++)
        {
            var unit = movable[i];
            var side = Vector3.Dot(unit.transform.position - center, right);
            members[i] = new FormationMember(IsMelee(unit), unit.GetComponent<UnitValues>().Rang, unit.GetInstanceID(), side);
        }

        var places = FormationRules.Arrange(members);

        for (var i = 0; i < movable.Count; i++)
        {
            formation[movable[i]] = (right * places[i].Right + forward * places[i].Forward) * ClosenessMultiplier;
        }

        return formation;
    }

    /// <summary>
    /// Only units that can move get a place in the formation, so anything else
    /// is sent straight to the point clicked (T-005).
    /// </summary>
    private static Vector3 GetFormationOffset(Dictionary<GameObject, Vector3> formation, GameObject unit)
    {
        return formation.TryGetValue(unit, out var offset) ? offset : Vector3.zero;
    }

    private static bool IsMelee(GameObject unit)
    {
        var behaviours = unit.GetComponent<UnitBehaviourManager>();
        return behaviours == null || !behaviours.Has<RangeAttackingBehaviour>();
    }

    private List<GameObject> GetMovableSelectedUnits()
    {
        return SelectedUnits
            .Where(x => x.GetComponent<UnitBehaviourManager>()?.Has<MovementBehaviour>() == true)
            .ToList();
    }
}
