using Assets.Scripts.Infrastructure.Events;

/// <summary>
/// A building that raises the supply limit — but only once it is finished.
/// </summary>
public class UnitSupplyProducer : UnitSupplyBase
{
    private Building _buildingScript;

    /// <summary>The limit was actually raised, so it is ours to lower again.</summary>
    private bool _given;

    protected override void Awake()
    {
        base.Awake();
        _buildingScript = gameObject.GetComponent<Building>();
    }

    private void OnEnable()
    {
        if (_unitEventManager == null)
        {
            return;
        }

        _unitEventManager.BuildingCompleted += OnBuildingCompletedHandler;
        _unitEventManager.UnitDied += RemoveMaxSupplyLimit;
    }

    private void OnDisable()
    {
        if (_unitEventManager == null)
        {
            return;
        }

        _unitEventManager.BuildingCompleted -= OnBuildingCompletedHandler;
        _unitEventManager.UnitDied -= RemoveMaxSupplyLimit;
    }

    void Start()
    {
        // A building still going up gives nothing yet: it will, on
        // BuildingCompleted.
        if (_unitValues.IsBuilding && _buildingScript != null && _buildingScript.BuildingIsInProgress)
        {
            return;
        }

        SetupResources();
    }

    protected void OnBuildingCompletedHandler(BuildingCompletedEventArgs args) => SetupResources();

    protected override void SetupResources()
    {
        if (_playerResources == null || _given)
        {
            return;
        }

        _given = true;
        AddMaxSupplyLimit();
    }

    protected void AddMaxSupplyLimit() => ProcessResources(
        (resourceName, amount) => _playerResources.AddResource(resourceName, amount, true),
        unitValues => unitValues.SupplyResourceProduces);

    protected void RemoveMaxSupplyLimit(DiedEventArgs args)
    {
        if (!_given)
        {
            return;
        }

        _given = false;

        ProcessResources(
            (resourceName, amount) => _playerResources.RemoveResource(resourceName, amount, true),
            unitValues => unitValues.SupplyResourceProduces);
    }
}
