using Assets.Scripts.Infrastructure.Events;

public class UnitSupplyProducer : UnitSupplyBase
{
    private Building? _buildingScript;

    protected override void Awake()
    {
        base.Awake();
        _buildingScript = gameObject.GetComponent<Building>();
        _unitEventManager = gameObject.GetComponent<UnitEventManager>();
    }

    void Start()
    {
        if (_unitValues.IsBuilding && _buildingScript?.BuildingIsInProgress == true)
        {
            _unitEventManager.BuildingCompleted += OnBuildingCompletedHandler;
        }
        else
        {
            SetupResources();
        }
    }

    protected void OnBuildingCompletedHandler(BuildingCompletedEventArgs args) => SetupResources();

    protected override void SetupResources()
    {
        if (_playerResources != null)
        {
            AddMaxSupplyLimit();
            _unitEventManager.UnitDied += RemoveMaxSupplyLimit;
        }
    }

    protected void AddMaxSupplyLimit() => ProcessResources(
        (resourceName, amount) => _playerResources.AddResource(resourceName, amount, true),
        unitValues => unitValues.SupplyResourceProduces);

    protected void RemoveMaxSupplyLimit(DiedEventArgs args) => ProcessResources(
        (resourceName, amount) => _playerResources.RemoveResource(resourceName, amount, true),
        unitValues => unitValues.SupplyResourceProduces);
}