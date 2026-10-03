using Assets.Scripts.Infrastructure.Enums;
using Assets.Scripts.Infrastructure.Events;
using System.Linq;
using UnityEngine;

public class BuildingController : MonoBehaviour
{
    private UnitsController _unitController;
    private BuildingGridController _buildingGridController;
    private PlayerResources _playerResources;
    private PlayerEventController _playerEventController;

    private bool _buildingMenuMod = false;
    public bool BuildingMenuMod { get { return _buildingMenuMod; } }

    private bool _buildingMod = false;
    public bool BuildingMod { get { return _buildingMod; } }

    /// <summary>The type chosen in the build menu, not a prefab.</summary>
    public UnitTypeData Building { get; set; } = null;

    public void Awake()
    {
        _unitController = gameObject.GetComponent<UnitsController>();
        _buildingGridController = gameObject.GetComponent<BuildingGridController>();
        _playerResources = gameObject.GetComponent<PlayerResources>();
        _playerEventController = GameObject.FindGameObjectWithTag(Tag.PlayerController.ToString())
            .GetComponent<PlayerEventController>();
    }

    public void EnableBuildingMenuMod()
    {
        var isBuilderSelected = _unitController.CheckBuilderSelected();

        if (isBuilderSelected)
        {
            SetBuildingMenuMod(true);
        }
    }

    public void DisableBuildingMenuMod()
    {
        SetBuildingMenuMod(false);
    }

    /// <summary>One place the menu flag changes, so the HUD hears every change.</summary>
    private void SetBuildingMenuMod(bool state)
    {
        if (_buildingMenuMod == state)
        {
            return;
        }

        _buildingMenuMod = state;
        _playerEventController.OnBuildingMenuModChanged(state);
    }

    public void EnableBuildingMod(KeyCode key)
    {
        var isBuilderSelected = _unitController.CheckBuilderSelected(out var builder);

        if (isBuilderSelected)
        {
            var building = builder.GetComponent<BuilderValues>()?.BuildingsToProduce
                .FirstOrDefault(x => x.KeyCode == key);

            if (building != null)
            {
                _buildingMod = true;
                Building = building.Building;
                SetBuildingMenuMod(false);
                _playerEventController.OnBuildingModChanged(_buildingMod);
            }
        }
    }

    public void DisableBuildingMod()
    {
        SetBuildingMenuMod(false);
        _buildingMod = false;
        _playerEventController.OnBuildingModChanged(_buildingMod);
    }

    public void Build(BuildActionStartedEventArgs eventArgs, int teamId, GameObject builder = null)
    {
        var building = eventArgs.Building;
        var point = eventArgs.Point;

        if (!CanBuild(building, point, builder)) return;
        if (!TrySpendResources(building)) return;

        var unit = InstantiateBuilding(building, point, teamId);
        unit.GetComponent<Building>().Build();

        _playerEventController.OnBuildingStarted(point, builder, unit);
        if (eventArgs.IsMineHeld)
        {
            unit.GetComponent<ResourceValues>().ResourcesAmount = eventArgs.MineToHeld.GetComponent<ResourceValues>().ResourcesAmount;
            HandleMineToHeld(eventArgs);
        }
    }

    private bool CanBuild(UnitTypeData building, Vector3 point, GameObject builder)
    {
        if (building.IsHeldMine)
            return true;

        if (!_buildingGridController.CheckIfCanBuildAt(point, building.Building.GridSize, builder))
        {
            Debug.Log("Can't build here!");
            return false;
        }
        return true;
    }

    private bool TrySpendResources(UnitTypeData building)
    {
        var resourceCost = building.Stats.ResourceCost.ToArray();

        if (!_playerResources.CheckIfCanSpendResources(resourceCost))
        {
            Debug.Log("Not enough resources!");
            return false;
        }

        if (!_playerResources.CheckIfHaveSupply(resourceCost))
        {
            Debug.Log("Not enough supply!");
            return false;
        }

        _playerResources.SpendResources(resourceCost);
        return true;
    }

    private GameObject InstantiateBuilding(UnitTypeData building, Vector3 point, int teamId)
    {
        var rotation = building.BodyPrefab.transform.rotation;

        // The building is lifted before it is switched on: moving it afterwards
        // would drag an already awake NavMeshObstacle across the map.
        return UnitFactory.Create(building, point, rotation, teamId,
            created => AdjustBuildingPosition(created, building));
    }

    private void AdjustBuildingPosition(GameObject unit, UnitTypeData type)
    {
        if (!type.IsHeldMine)
        {
            float offsetY = unit.transform.localScale.y / 2f;
            unit.transform.position += new Vector3(0, offsetY, 0);
        }
    }

    private void HandleMineToHeld(BuildActionStartedEventArgs eventArgs)
    {
        if (eventArgs.MineToHeld != null)
        {
            _playerEventController.OnBuildingRemoved(eventArgs.MineToHeld);
            Destroy(eventArgs.MineToHeld);
        }
    }
}
