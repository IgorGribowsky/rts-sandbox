using Assets.Scripts.Infrastructure.Enums;
using Assets.Scripts.Infrastructure.Events;
using System.Linq;
using UnityEngine;

public class UnitSupplyRequirement : UnitSupplyBase
{
    void Start()
    {
        SetupResources();
    }

    protected override void SetupResources()
    {
        if (_playerResources != null)
        {
            AddSupplyLimit();
            _unitEventManager.UnitDied += RemoveSupplyLimit;
        }
    }

    protected void AddSupplyLimit() => ProcessResources(
        (resourceName, amount) => _playerResources.AddResource(resourceName, amount),
        unitValues => unitValues.ResourceCost);

    protected void RemoveSupplyLimit(DiedEventArgs args) => ProcessResources(
        (resourceName, amount) => _playerResources.RemoveResource(resourceName, amount),
        unitValues => unitValues.ResourceCost);
}
