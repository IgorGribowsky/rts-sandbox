using Assets.Scripts.Infrastructure.Constants;
using Assets.Scripts.Infrastructure.Enums;
using Assets.Scripts.Infrastructure.Events;
using Assets.Scripts.Infrastructure.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BuildingGridController : MonoBehaviour
{
    public GameObject GridSegment;
    public Vector3 startGridPoint = new Vector3(-50f, 0.1f, -50f);
    public Vector2 gridSize = new Vector2(100f, 100f);

    private List<GridForBuilding> _gridForBuildings = new List<GridForBuilding>();
    private GameObject cursorGrid;
    private BuildingController _buildingController;
    private PlayerEventController _playerEventController;

    private GameObject _unitUnderCursor { get; set; }
    private Vector3 _mousePosition { get; set; }

    private bool isMineUnderCursor = false;

    public void Awake()
    {
        _buildingController = GetComponent<BuildingController>();

        _playerEventController = GameObject.FindGameObjectWithTag(Tag.PlayerController.ToString())
            .GetComponent<PlayerEventController>();

        _playerEventController.BuildingStarted += AddToGrid;
        _playerEventController.BuildingRemoved += RemoveFromGrid;
        _playerEventController.BuildingModChanged += BuildingModChangedHandler;
        _playerEventController.CursorMoved += CursorMovedHandler;
    }

    public void Start()
    {
        cursorGrid = new GameObject();
        GenerateRestrictedGridCells();
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

        GenerateGridForBuilding(buildingStartedEventArgs.Building, buildingStartedEventArgs.Point, buildingValues);
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
        var currentBuildingValues = _buildingController.Building.GetComponent<BuildingValues>();

        var isMine = false;

        if (buildingValues != null && currentBuildingValues != null && buildingValues.IsResource && currentBuildingValues.IsResource)
        {
            var resourceValues = _unitUnderCursor.GetComponent<ResourceValues>();
            var currentResourceValues = _buildingController.Building.GetComponent<ResourceValues>();
            isMine = resourceValues.IsMine && currentResourceValues.IsHeldMine && resourceValues.ResourceName == currentResourceValues.ResourceName;
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

    private void HandleModEnabled()
    {
        var buildingValues = _buildingController.Building.GetComponent<BuildingValues>();

        UpdateCursorPosition();
        GenerateGridForCursor(cursorGrid, buildingValues.GridSize, buildingValues.IsHeldMine);
    }

    private void UpdateCursorPosition()
    {
        var buildingValues = _buildingController.Building.GetComponent<BuildingValues>();
        isMineUnderCursor = CheckIfMineUnderCursor();

        if (isMineUnderCursor)
        {
            cursorGrid.transform.position = _unitUnderCursor.transform.position;
        }
        else
        {
            var gridSize = buildingValues.GridSize;
            cursorGrid.transform.position = _mousePosition.GetGridPoint(gridSize);
        }

        if (buildingValues.IsHeldMine)
        {
            foreach (Transform gridSegment in cursorGrid.transform)
            {
                gridSegment.GetComponent<GridSegment>().Restricted = !isMineUnderCursor;
            }
        }
    }

    private void DestroyGridForCursor()
    {
        foreach (Transform gridSegment in cursorGrid.transform)
        {
            Destroy(gridSegment.gameObject);
        }
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

                GenerateGridForBuilding(collider.gameObject, collider.transform.position, buildingValues);
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

                GenerateGridForBuilding(collider.gameObject, collider.transform.position, buildingValues);
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
                gridSegmentScript.Restricted = isHeldMine && !isMineUnderCursor;
                gridSegmentScript.ShowOrHideSegment(_buildingController.BuildingMod);
            }
        }
    }

    private void GenerateGridForBuilding(GameObject building, Vector3 buildingPosition, BuildingValues buildingValues)
    {
        var buildingSize = buildingValues.GridSize;
        var shift = buildingSize / 2.0f;

        var gridForBuilding = new GridForBuilding()
        {
            GridSegments = new List<GameObject>(),
            Building = building,
        };

        _gridForBuildings.Add(gridForBuilding);

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
                gridSegmentScript.Restricted = true;
                gridSegmentScript.ShowOrHideSegment(_buildingController.BuildingMod);
                gridForBuilding.GridSegments.Add(gridSegment);
            }
        }
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
}
