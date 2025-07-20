using Assets.Scripts.Infrastructure.Events;
using System.Linq;
using UnityEngine;

public class BuildingController : MonoBehaviour
{
    private UnitsController _unitController;
    private BuildingGridController _buildingGridController;
    private PlayerResources _playerResources;

    private bool _buildingMenuMod = false;
    public bool BuildingMenuMod { get { return _buildingMenuMod; } }

    private bool _buildingMod = false;
    public bool BuildingMod { get { return _buildingMod; } }

    public GameObject Building { get; set; } = null;

    //TODO: move to global event manager
    public event ModStateChangedHandler BuildingModChanged;
    public void OnBuildingModChanged(bool state)
    {
        BuildingModChanged?.Invoke(new ModStateChangedEventArgs(state));
    }

    //TODO: move to global event manager
    public event BuildingStartedHandler BuildingStarted;
    public void OnBuildingStarted(Vector3 point, GameObject builder, GameObject building)
    {
        BuildingStarted?.Invoke(new BuildingStartedEventArgs(point, builder, building));
    }

    //TODO: move to global event manager
    public event BuildingRemovedHandler BuildingRemoved;
    public void OnBuildingRemoved(GameObject building)
    {
        BuildingRemoved?.Invoke(new BuildingRemovedEventArgs(building));
    }

    public void Awake()
    {
        _unitController = gameObject.GetComponent<UnitsController>();
        _buildingGridController = gameObject.GetComponent<BuildingGridController>();
        _playerResources = gameObject.GetComponent<PlayerResources>();
    }

    public void EnableBuildingMenuMod()
    {
        var isBuilderSelected = _unitController.CheckBuilderSelected();

        if (isBuilderSelected)
        {
            _buildingMenuMod = true;
        }
    }

    public void DisableBuildingMenuMod()
    {
        _buildingMenuMod = false;
    }

    public void EnableBuildingMod(KeyCode key)
    {
        var isBuilderSelected = _unitController.CheckBuilderSelected(out var builder);

        if (isBuilderSelected)
        {
            var building = builder.GetComponent<UnitValues>().BuildingsToProduce.FirstOrDefault(x => x.KeyCode == key);

            if (building != null)
            {
                _buildingMod = true;
                _buildingMenuMod = false;
                Building = building.Building;
                OnBuildingModChanged(_buildingMod);
            }
        }
    }

    public void DisableBuildingMod()
    {
        _buildingMenuMod = false;
        _buildingMod = false;
        OnBuildingModChanged(_buildingMod);
    }

    public void Build(BuildActionStartedEventArgs eventArgs, int teamId, GameObject builder = null)
    {
        var building = eventArgs.Building;
        var point = eventArgs.Point;

        if (!CanBuild(building, point, builder)) return;
        if (!TrySpendResources(building)) return;

        var unit = InstantiateBuilding(building, point);
        unit.GetComponent<TeamMember>().TeamId = teamId;
        unit.GetComponent<Building>().Build();

        OnBuildingStarted(point, builder, unit);
        if (eventArgs.IsMineHeld)
        {
            unit.GetComponent<ResourceValues>().ResourcesAmount = eventArgs.MineToHeld.GetComponent<ResourceValues>().ResourcesAmount;
            HandleMineToHeld(eventArgs);
        }
    }

    private bool CanBuild(GameObject building, Vector3 point, GameObject builder)
    {
        var buildingValues = building.GetComponent<BuildingValues>();
        if (buildingValues.IsHeldMine)
            return true;

        if (!_buildingGridController.CheckIfCanBuildAt(point, buildingValues.GridSize, builder))
        {
            Debug.Log("Can't build here!");
            return false;
        }
        return true;
    }

    private bool TrySpendResources(GameObject building)
    {
        var resourceCost = building.GetComponent<UnitValues>().ResourceCost.ToArray();

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

    private GameObject InstantiateBuilding(GameObject building, Vector3 point)
    {
        var unit = Instantiate(building, point, building.transform.rotation);
        AdjustBuildingPosition(unit);
        return unit;
    }

    private void AdjustBuildingPosition(GameObject unit)
    {
        var buildingValues = unit.GetComponent<BuildingValues>();
        if (!buildingValues.IsHeldMine)
        {
            float offsetY = unit.transform.localScale.y / 2f;
            unit.transform.position += new Vector3(0, offsetY, 0);
        }
    }

    private void HandleMineToHeld(BuildActionStartedEventArgs eventArgs)
    {
        if (eventArgs.MineToHeld != null)
        {
            OnBuildingRemoved(eventArgs.MineToHeld);
            Destroy(eventArgs.MineToHeld);
        }
    }
}
