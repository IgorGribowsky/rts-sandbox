using Assets.Scripts.Infrastructure.Events;

/// <summary>
/// A unit eats supply while it lives and frees it when it dies.
/// </summary>
public class UnitSupplyRequirement : UnitSupplyBase
{
    /// <summary>
    /// The limit was actually taken. Guards against giving back supply that was
    /// never taken — this unit may belong to a team with no player behind it.
    /// </summary>
    private bool _taken;

    private void OnEnable()
    {
        if (_unitEventManager != null)
        {
            _unitEventManager.UnitDied += RemoveSupplyLimit;
        }
    }

    private void OnDisable()
    {
        if (_unitEventManager != null)
        {
            _unitEventManager.UnitDied -= RemoveSupplyLimit;
        }
    }

    void Start()
    {
        SetupResources();
    }

    protected override void SetupResources()
    {
        if (_playerResources == null || _taken)
        {
            return;
        }

        _taken = true;
        AddSupplyLimit();
    }

    protected void AddSupplyLimit() => ProcessResources(
        (resourceName, amount) => _playerResources.AddResource(resourceName, amount),
        unitValues => unitValues.ResourceCost);

    protected void RemoveSupplyLimit(DiedEventArgs args)
    {
        if (!_taken)
        {
            return;
        }

        _taken = false;

        ProcessResources(
            (resourceName, amount) => _playerResources.RemoveResource(resourceName, amount),
            unitValues => unitValues.ResourceCost);
    }
}
