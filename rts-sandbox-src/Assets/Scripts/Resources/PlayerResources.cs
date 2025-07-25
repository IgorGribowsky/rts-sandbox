using Assets.Scripts.Infrastructure.Enums;
using Assets.Scripts.Infrastructure.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerResources : MonoBehaviour
{
    public List<ResourceAmount> ResourcesAmount = new List<ResourceAmount>();

    public List<ResourceAmount> MaxSupplyResourcesAmount = new List<ResourceAmount>();

    private GameResources _gameResources;
    private PlayerEventController _playerEventController;

    void Awake()
    {
        var gameController = GameObject.FindGameObjectWithTag(Tag.GameController.ToString());
        _gameResources = gameController.GetComponent<GameResources>();
        _playerEventController = GameObject.FindGameObjectWithTag(Tag.PlayerController.ToString())
            .GetComponent<PlayerEventController>();
    }

    void Start()
    {
    }

    void Update()
    {

    }

    public void AddResource(ResourceName resourceName, int amount, bool isMaxSupplyResource = false)
    {
        UpdateResourceAmount(resourceName, amount, isMaxSupplyResource, (current, change) => current + change);
    }

    public void RemoveResource(ResourceName resourceName, int amount, bool isMaxSupplyResource = false)
    {
        UpdateResourceAmount(resourceName, amount, isMaxSupplyResource, (current, change) => current - change);
    }

    private void UpdateResourceAmount(
        ResourceName resourceName,
        int amount,
        bool isMaxSupplyResource,
        Func<int, int, int> updateOperation)
    {
        var resourceAmount = (isMaxSupplyResource
            ? MaxSupplyResourcesAmount
            : ResourcesAmount).FirstOrDefault(x => x.ResourceName == resourceName);

        var oldValue = resourceAmount.Amount;
        resourceAmount.Amount = updateOperation(oldValue, amount);
        var newValue = resourceAmount.Amount;

        var gameResource = _gameResources.Resources
            .FirstOrDefault(x => x.ResourceName == resourceName);

        _playerEventController.OnResourceChanged(resourceName, gameResource.ResourceType, oldValue, newValue);
    }

    public bool CheckIfCanSpendResources(params ResourceAmount[] resourceAmounts)
    {
        return ValidateResources(resourceAmounts, (playerResource, gameResource, requiredAmount) =>
            gameResource.ResourceType == ResourceType.SupplyResource || playerResource.Amount >= requiredAmount);
    }

    public bool CheckIfHaveSupply(params ResourceAmount[] resourceAmounts)
    {
        return ValidateResources(resourceAmounts, (playerResource, gameResource, requiredAmount) =>
        {
            if (gameResource.ResourceType == ResourceType.SupplyResource)
            {
                var playerMaxSupply = MaxSupplyResourcesAmount
                    .FirstOrDefault(x => x.ResourceName == gameResource.ResourceName);

                return playerMaxSupply != null && playerResource.Amount + requiredAmount <= playerMaxSupply.Amount;
            }
            return true;
        });
    }

    /// <summary>
    /// Универсальный метод для валидации ресурсов на основе переданной логики проверки.
    /// </summary>
    private bool ValidateResources(ResourceAmount[] resourceAmounts, Func<ResourceAmount, Resource, int, bool> validationLogic)
    {
        foreach (var resource in resourceAmounts)
        {
            var gameResource = _gameResources.Resources
                .FirstOrDefault(x => x.ResourceName == resource.ResourceName);

            var playerResource = ResourcesAmount
                .FirstOrDefault(x => x.ResourceName == resource.ResourceName);

            if (gameResource == null || playerResource == null)
            {
                return false;
            }

            if (!validationLogic(playerResource, gameResource, resource.Amount))
            {
                return false;
            }
        }
        return true;
    }

    public void SpendResources(params ResourceAmount[] resourceAmounts)
    {
        foreach (var resource in resourceAmounts)
        {
            var gameResource = _gameResources.Resources.FirstOrDefault(x => x.ResourceName == resource.ResourceName);
            if (gameResource.ResourceType == ResourceType.SupplyResource)
            {
                continue;
            }

            var playerResource = ResourcesAmount.First(x => x.ResourceName == resource.ResourceName);

            var oldValue = playerResource.Amount;
            playerResource.Amount -= resource.Amount;
            var newValue = playerResource.Amount;

            _playerEventController.OnResourceChanged(gameResource.ResourceName, gameResource.ResourceType, oldValue, newValue);
        }
    }
}

[Serializable]
public class ResourceAmount
{
    public ResourceName ResourceName;

    public int Amount;
}
