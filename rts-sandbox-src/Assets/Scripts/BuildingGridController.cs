using Assets.Scripts.Infrastructure.Abstractions;
using Assets.Scripts.Infrastructure.Constants;
using Assets.Scripts.Infrastructure.Enums;
using Assets.Scripts.Infrastructure.Events;
using Assets.Scripts.Infrastructure.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// The build grid (M-010): cells where nothing may be built, the cells under
/// the cursor while a building is being placed, the ghost of that building,
/// and the ghosts of buildings queued with Shift.
///
/// Every cell under the cursor shows for itself whether it is free (T-060):
/// a red cell is the one in the way. The ghost turns red when any cell is.
/// </summary>
public class BuildingGridController : MonoBehaviour
{
    /// <summary>How often the cells under a still cursor look again: units walk in and out.</summary>
    private const float OccupancyRecheckSeconds = 0.2f;

    private const float GhostAlpha = 0.5f;
    private const float QueuedGhostAlpha = 0.26f;

    /// <summary>
    /// The team colour is thinned with white for a ghost that fits, and a
    /// blocked one is a deep, solid red: a red team still tells them apart.
    /// </summary>
    private const float GhostWhitening = 0.4f;
    private static readonly Color BlockedGhostColor = new Color(0.72f, 0.02f, 0.02f, 0.8f);

    /// <summary>Cells around every building are background: fainter than the ones under the cursor.</summary>
    private const float BackgroundCellOpacity = 0.45f;

    public GameObject GridSegment;
    public Vector3 startGridPoint = new Vector3(-50f, 0.1f, -50f);
    public Vector2 gridSize = new Vector2(100f, 100f);

    public Material BuildingAllowedMaterial;
    public Material BuildingRestrictedMaterial;
    public Material BuildingShadowMaterial;

    [Tooltip("See-through look of a building not built yet: under the cursor and queued with Shift (T-060).")]
    public Material BuildingGhostMaterial;

    private List<GridForBuilding> _gridForBuildings = new List<GridForBuilding>();
    private List<GridForShadow> _gridForShadows = new List<GridForShadow>();
    private GameObject cursorGrid;
    private readonly List<GridSegment> _cursorCells = new List<GridSegment>();
    private BuildingGhost _cursorGhost;
    private Vector3 _lastCursorPoint = new Vector3(float.NaN, 0f, 0f);
    private float _nextOccupancyCheck;
    private Color _teamColor = Color.white;
    private UnitsController _unitsController;
    private BuildingController _buildingController;
    private PlayerEventController _playerEventController;

    private GameObject _unitUnderCursor { get; set; }
    private Vector3 _mousePosition { get; set; }

    private bool isMineUnderCursor = false;

    public void Awake()
    {
        _buildingController = GetComponent<BuildingController>();
        _unitsController = GetComponent<UnitsController>();

        _playerEventController = GameObject.FindGameObjectWithTag(Tag.PlayerController.ToString())
            .GetComponent<PlayerEventController>();
    }

    private void OnEnable()
    {
        if (_playerEventController == null)
        {
            return;
        }

        _playerEventController.BuildingStarted += AddToGrid;
        _playerEventController.BuildingRemoved += RemoveFromGrid;
        _playerEventController.BuildingModChanged += BuildingModChangedHandler;
        _playerEventController.CursorMoved += CursorMovedHandler;

        _playerEventController.CurrentCommandEnded += RemoveShadow;
        _playerEventController.CommandAddedToQueue += AddShadow;
        _playerEventController.CommandsQueueCleared += RemoveShadows;
    }

    private void OnDisable()
    {
        if (_playerEventController == null)
        {
            return;
        }

        _playerEventController.BuildingStarted -= AddToGrid;
        _playerEventController.BuildingRemoved -= RemoveFromGrid;
        _playerEventController.BuildingModChanged -= BuildingModChangedHandler;
        _playerEventController.CursorMoved -= CursorMovedHandler;

        _playerEventController.CurrentCommandEnded -= RemoveShadow;
        _playerEventController.CommandAddedToQueue -= AddShadow;
        _playerEventController.CommandsQueueCleared -= RemoveShadows;
    }

    public void Start()
    {
        cursorGrid = new GameObject("Building Cursor Grid");
        GenerateRestrictedGridCells();

        var teamId = GetComponent<PlayerTeamMember>()?.TeamId;
        var team = Assets.Scripts.GameServices.TeamController?.Teams?.FirstOrDefault(t => t.Id == teamId);
        if (team != null)
        {
            _teamColor = Color.Lerp(team.Color, Color.white, GhostWhitening);
        }
    }

    /// <summary>A unit may walk onto a cell while the cursor stands still.</summary>
    private void LateUpdate()
    {
        if (!_buildingController.BuildingMod || _cursorCells.Count == 0 || Time.time < _nextOccupancyCheck)
        {
            return;
        }

        RefreshCursorCells();
    }

    public bool CheckIfCanBuildAt(Vector3 point, int size, GameObject builder = null)
    {
        Collider[] colliders = Physics.OverlapBox(
            point,
            Vector3.one * size * GameConstants.GridCellSize / 2,
            Quaternion.identity
        );

        foreach (var collider in colliders)
        {
            if (collider.gameObject == builder)
            {
                continue;
            }

            if (collider.CompareTag(Tag.Unit.ToString())
                || (collider.CompareTag(Tag.GridSegment.ToString()) && collider.GetComponent<GridSegment>().Restricted))
            {
                return false;
            }
        }

        return true;
    }

    protected void AddToGrid(BuildingStartedEventArgs buildingStartedEventArgs)
    {
        var buildingValues = buildingStartedEventArgs.Building.GetComponent<BuildingValues>();

        if (buildingValues == null)
        {
            return;
        }

        var gridForBuilding = GenerateGridForBuilding(buildingStartedEventArgs.Building, buildingStartedEventArgs.Point, buildingValues.GridSize, BuildingRestrictedMaterial);
        _gridForBuildings.Add(gridForBuilding);
    }

    protected void RemoveFromGrid(BuildingRemovedEventArgs buildingRemovedEventArgs)
    {
        var grid = _gridForBuildings.FirstOrDefault(x => x.Building == buildingRemovedEventArgs.Building);

        if (grid != null) 
        {
            foreach (var gridSegment in grid.GridSegments)
            {
                Destroy(gridSegment);
            }
        }

        _gridForBuildings.Remove(grid);
    }

    public bool CheckIfMineUnderCursor()
    {
        var buildingValues = _unitUnderCursor?.GetComponent<BuildingValues>();
        var currentType = _buildingController.Building;

        var isMine = false;

        if (buildingValues != null && currentType != null && buildingValues.IsResource && currentType.IsResourceObject)
        {
            var resourceValues = _unitUnderCursor.GetComponent<ResourceValues>();
            isMine = resourceValues.IsMine && currentType.IsHeldMine && resourceValues.Resource == currentType.Resource;
        }

        return isMine;
    }

    protected void CursorMovedHandler(CursorMovedEventArgs args)
    {
        _mousePosition = args.CursorPosition;
        _unitUnderCursor = args.UnitUnderCursor;

        if (_buildingController.BuildingMod)
        {
            UpdateCursorPosition();
        }
    }

    protected void BuildingModChangedHandler(ModStateChangedEventArgs modStateChangedEventArgs)
    {
        if (modStateChangedEventArgs.State)
        {
            HandleModEnabled();
        }
        else
        {
            DestroyGridForCursor();
        }
    }

    protected void AddShadow(CommandAddedToQueueEventArgs args)
    {
        if (args.Command is not IBuildCommand buildCommand)
            return;

        var buildingType = buildCommand.GetBuildingType();
        var point = buildCommand.GetPoint();

        GridForBuilding gridForBuilding = GenerateGridForBuilding(null, point, buildingType.Building.GridSize, BuildingShadowMaterial, false);
        GridForShadow gridForShadow = ConvertBuildingGridToShadowGrid(buildCommand, gridForBuilding);

        if (BuildingGhostMaterial != null)
        {
            gridForShadow.Ghost = new BuildingGhost(buildingType, BuildingGhostMaterial);
            gridForShadow.Ghost.Place(point);
            gridForShadow.Ghost.SetColor(WithAlpha(_teamColor, QueuedGhostAlpha));
        }

        _gridForShadows.Add(gridForShadow);
    }

    private static GridForShadow ConvertBuildingGridToShadowGrid(IBuildCommand buildCommand, GridForBuilding gridForBuilding)
    {
        return new GridForShadow()
        {
            Building = gridForBuilding.Building,
            GridSegments = gridForBuilding.GridSegments,
            Command = buildCommand
        };
    }

    protected void RemoveShadow(CurrentCommandEndedEventArgs args)
    {
        if (args.Command is not IBuildCommand buildCommand)
            return;

        var grid = _gridForShadows.FirstOrDefault(x => x.Command == buildCommand);

        if (grid != null)
        {
            foreach (var gridSegment in grid.GridSegments)
            {
                Destroy(gridSegment);
            }

            grid.Ghost?.Destroy();
        }

        _gridForShadows.Remove(grid);
    }

    protected void RemoveShadows(CommandsQueueClearedEventArgs args)
    {
        foreach (var command in args.Commands)
        {
            RemoveShadow(new CurrentCommandEndedEventArgs(command));
        }
    }

    private void HandleModEnabled()
    {
        var buildingType = _buildingController.Building;

        // Another building chosen while placing one: the old cells and ghost go.
        DestroyGridForCursor();

        GenerateGridForCursor(cursorGrid, buildingType.Building.GridSize, buildingType.IsHeldMine);
        if (BuildingGhostMaterial != null)
        {
            _cursorGhost = new BuildingGhost(buildingType, BuildingGhostMaterial);
        }

        _lastCursorPoint = new Vector3(float.NaN, 0f, 0f);
        UpdateCursorPosition();
    }

    private void UpdateCursorPosition()
    {
        var buildingType = _buildingController.Building;
        isMineUnderCursor = CheckIfMineUnderCursor();

        if (isMineUnderCursor)
        {
            cursorGrid.transform.position = _unitUnderCursor.transform.position;
        }
        else
        {
            var gridSize = buildingType.Building.GridSize;
            cursorGrid.transform.position = _mousePosition.GetGridPoint(gridSize);
        }

        // The cells are looked at again only when the grid point moves, not on
        // every pixel of the mouse.
        if (cursorGrid.transform.position != _lastCursorPoint)
        {
            _lastCursorPoint = cursorGrid.transform.position;
            RefreshCursorCells();
        }
    }

    /// <summary>
    /// Each cell under the cursor red or light by itself, the ghost red if any
    /// cell is. A held mine goes only onto a mine: all its cells follow that.
    /// </summary>
    private void RefreshCursorCells()
    {
        _nextOccupancyCheck = Time.time + OccupancyRecheckSeconds;

        var buildingType = _buildingController.Building;
        if (buildingType == null)
        {
            return;
        }

        GameObject builder = null;
        _unitsController?.CheckBuilderSelected(out builder);

        var size = buildingType.Building.GridSize;
        var anyBlocked = false;

        foreach (var cell in _cursorCells)
        {
            if (cell == null)
            {
                continue;
            }

            var blocked = buildingType.IsHeldMine
                ? !isMineUnderCursor
                : IsCellBlocked(cell.transform.position, size, builder);

            cell.Restricted = buildingType.IsHeldMine && blocked;
            cell.SetMaterial(blocked ? BuildingRestrictedMaterial : BuildingAllowedMaterial);
            anyBlocked |= blocked;
        }

        if (_cursorGhost != null)
        {
            _cursorGhost.Place(cursorGrid.transform.position);
            _cursorGhost.SetColor(anyBlocked ? BlockedGhostColor : WithAlpha(_teamColor, GhostAlpha));
        }
    }

    /// <summary>The same test as CheckIfCanBuildAt, for one cell of the building.</summary>
    private bool IsCellBlocked(Vector3 cellPosition, int size, GameObject builder)
    {
        var cell = GameConstants.GridCellSize;
        var center = new Vector3(cellPosition.x, cursorGrid.transform.position.y, cellPosition.z);
        var halfExtents = new Vector3(cell / 2f - 0.05f, size * cell / 2f, cell / 2f - 0.05f);

        foreach (var collider in Physics.OverlapBox(center, halfExtents, Quaternion.identity))
        {
            if (collider.gameObject == builder)
            {
                continue;
            }

            if (collider.CompareTag(Tag.Unit.ToString())
                || (collider.CompareTag(Tag.GridSegment.ToString()) && collider.GetComponent<GridSegment>().Restricted))
            {
                return true;
            }
        }

        return false;
    }

    private static Color WithAlpha(Color color, float alpha)
    {
        color.a = alpha;
        return color;
    }

    private void DestroyGridForCursor()
    {
        foreach (Transform gridSegment in cursorGrid.transform)
        {
            Destroy(gridSegment.gameObject);
        }

        _cursorCells.Clear();
        _cursorGhost?.Destroy();
        _cursorGhost = null;
    }

    private void GenerateRestrictedGridCells()
    {
        Vector3 bottomLeft = new Vector3(startGridPoint.x, 0f, startGridPoint.z);
        Vector3 topRight = bottomLeft + new Vector3(gridSize.x, 1f, gridSize.y);

        GenerateForBuildings(bottomLeft, topRight);

        GenerateForStaticObjects(bottomLeft, topRight);

        GenerateForHarvestedResources(bottomLeft, topRight);
    }

    private void GenerateForBuildings(Vector3 bottomLeft, Vector3 topRight)
    {
        Collider[] colliders = Physics.OverlapBox((bottomLeft + topRight) / 2, (topRight - bottomLeft) / 2, Quaternion.identity);

        foreach (var collider in colliders)
        {
            if (collider.CompareTag(Tag.Unit.ToString()))
            {
                var buildingValues = collider.gameObject.GetComponent<BuildingValues>();

                if (buildingValues == null)
                {
                    continue;
                }

                var gridForBuilding = GenerateGridForBuilding(collider.gameObject, collider.transform.position, buildingValues.GridSize, BuildingRestrictedMaterial);
                _gridForBuildings.Add(gridForBuilding);
            }
        }
    }

    private void GenerateForHarvestedResources(Vector3 bottomLeft, Vector3 topRight)
    {
        Collider[] colliders = Physics.OverlapBox((bottomLeft + topRight) / 2, (topRight - bottomLeft) / 2, Quaternion.identity);

        foreach (var collider in colliders)
        {
            if (collider.CompareTag(Tag.HarvestedResource.ToString()))
            {
                var buildingValues = collider.gameObject.GetComponent<BuildingValues>();

                if (buildingValues == null)
                {
                    continue;
                }

                var gridForBuilding = GenerateGridForBuilding(collider.gameObject, collider.transform.position, buildingValues.GridSize, BuildingRestrictedMaterial);
                _gridForBuildings.Add(gridForBuilding);
            }
        }
    }

    private void GenerateGridForCursor(GameObject cursorGameObject, int gridSize, bool isHeldMine)
    {
        var shift = gridSize / 2.0f;

        for (float i = -shift; i < shift; i += GameConstants.GridCellSize)
        {
            for (float j = -shift; j < shift; j += GameConstants.GridCellSize)
            {
                var position = new Vector3(i, 0f, j)
                    + new Vector3(0.5f, 0f, 0.5f) * GameConstants.GridCellSize;

                var yCorrectedPosition = new Vector3(position.x, startGridPoint.y, position.z);

                var gridSegment = Instantiate(GridSegment);
                gridSegment.transform.SetParent(cursorGameObject.transform);
                gridSegment.transform.localPosition = yCorrectedPosition;

                var gridSegmentScript = gridSegment.GetComponent<GridSegment>();
                var isMineUnderCursor = CheckIfMineUnderCursor();

                var isRestricted = isHeldMine && !isMineUnderCursor;
                gridSegmentScript.Restricted = isRestricted;
                var material = isRestricted ? BuildingRestrictedMaterial : BuildingAllowedMaterial;
                gridSegmentScript.SetMaterial(material);

                gridSegmentScript.ShowOrHideSegment(_buildingController.BuildingMod);
                _cursorCells.Add(gridSegmentScript);
            }
        }
    }

    /// <summary>
    /// <paramref name="building"/> is only the key the grid is remembered by,
    /// and is null for the shadow of a queued order: there is no object yet,
    /// only the type. The shadow is found by its command instead.
    /// </summary>
    private GridForBuilding GenerateGridForBuilding(
        GameObject building,
        Vector3 buildingPosition,
        int buildingSize,
        Material material,
        bool isRestricted = true)
    {
        var shift = buildingSize / 2.0f;

        var gridForBuilding = new GridForBuilding()
        {
            GridSegments = new List<GameObject>(),
            Building = building,
        };

        for (float i = -shift; i < shift; i += GameConstants.GridCellSize)
        {
            for (float j = -shift; j < shift; j += GameConstants.GridCellSize)
            {
                var position = buildingPosition
                    + new Vector3(i, 0f, j)
                    + new Vector3(0.5f, 0f, 0.5f) * GameConstants.GridCellSize;

                var yCorrectedPosition = new Vector3(position.x, startGridPoint.y, position.z);

                var gridSegment = Instantiate(GridSegment, yCorrectedPosition, Quaternion.identity);
                var gridSegmentScript = gridSegment.GetComponent<GridSegment>();
                gridSegmentScript.Restricted = isRestricted;
                gridSegmentScript.SetMaterial(material, isRestricted ? BackgroundCellOpacity : 1f);
                gridSegmentScript.ShowOrHideSegment(_buildingController.BuildingMod);
                gridForBuilding.GridSegments.Add(gridSegment);
            }
        }

        return gridForBuilding;
    }

    private void GenerateForStaticObjects(Vector3 bottomLeft, Vector3 topRight)
    {
        for (float x = bottomLeft.x; x < topRight.x; x += GameConstants.GridCellSize)
        {
            for (float z = bottomLeft.z; z < topRight.z; z += GameConstants.GridCellSize)
            {
                var cellPosition = new Vector3(x + GameConstants.GridCellSize / 2, startGridPoint.y, z + GameConstants.GridCellSize / 2);

                if (IsColliding(cellPosition, hit => hit.CompareTag(Tag.Obstacle.ToString())))
                {
                    var gridSegment = Instantiate(GridSegment, cellPosition, Quaternion.identity);
                    var gridSegmentScript = gridSegment.GetComponent<GridSegment>();
                    gridSegmentScript.Restricted = true;
                    gridSegmentScript.SetMaterial(BuildingRestrictedMaterial, BackgroundCellOpacity);
                    gridSegmentScript.ShowOrHideSegment(_buildingController.BuildingMod);
                }
            }
        }
    }

    private bool IsColliding(Vector3 position, Func<Collider, bool> func)
    {
        Collider[] hits = Physics.OverlapBox(position, new Vector3(GameConstants.GridCellSize / 2, 1f, GameConstants.GridCellSize / 2), Quaternion.identity);
        return hits.Any(func);
    }

    private class GridForBuilding
    {
        public GameObject Building { get; set; }

        public List<GameObject> GridSegments { get; set; }
    }

    private class GridForShadow : GridForBuilding
    {
        public IBuildCommand Command { get; set; }

        public BuildingGhost Ghost { get; set; }
    }

}
