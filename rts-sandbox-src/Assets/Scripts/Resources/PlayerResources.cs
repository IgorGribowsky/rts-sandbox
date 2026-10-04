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

        AddMissingResources();
    }

    /// <summary>
    /// A resource asset put into GameResources starts at zero here by itself
    /// (T-052): a new resource needs no entry in the scene to be counted. A
    /// supply resource gets a zero limit too, which farms then raise.
    /// </summary>
    private void AddMissingResources()
    {
        foreach (var resource in _gameResources.Definitions)
        {
            if (resource == null)
            {
                continue;
            }

            if (!ResourcesAmount.Any(x => x.Resource == resource))
            {
                ResourcesAmount.Add(new ResourceAmount { Resource = resource, Amount = 0 });
            }

            if (resource.IsSupply && !MaxSupplyResourcesAmount.Any(x => x.Resource == resource))
            {
                MaxSupplyResourcesAmount.Add(new ResourceAmount { Resource = resource, Amount = 0 });
            }
        }
    }

    public void AddResource(ResourceDefinition resource, int amount, bool isMaxSupplyResource = false)
    {
        UpdateResourceAmount(resource, amount, isMaxSupplyResource, (current, change) => current + change);
    }

    public void RemoveResource(ResourceDefinition resource, int amount, bool isMaxSupplyResource = false)
    {
        UpdateResourceAmount(resource, amount, isMaxSupplyResource, (current, change) => current - change);
    }

    private void UpdateResourceAmount(
        ResourceDefinition resource,
        int amount,
        bool isMaxSupplyResource,
        Func<int, int, int> updateOperation)
    {
        var resourceAmount = (isMaxSupplyResource
            ? MaxSupplyResourcesAmount
            : ResourcesAmount).FirstOrDefault(x => x.Resource == resource);

        // A resource the game does not have — not in GameResources — is not counted.
        if (resourceAmount == null)
        {
            return;
        }

        var oldValue = resourceAmount.Amount;
        resourceAmount.Amount = updateOperation(oldValue, amount);
        var newValue = resourceAmount.Amount;

        _playerEventController.OnResourceChanged(resource, resource.Type, oldValue, newValue);
    }

    public bool CheckIfCanSpendResources(params ResourceAmount[] resourceAmounts)
    {
        return ValidateResources(resourceAmounts, (playerResource, gameResource, requiredAmount) =>
            gameResource.Type == ResourceType.SupplyResource || playerResource.Amount >= requiredAmount);
    }

    public bool CheckIfHaveSupply(params ResourceAmount[] resourceAmounts)
    {
        return ValidateResources(resourceAmounts, (playerResource, gameResource, requiredAmount) =>
        {
            if (gameResource.Type == ResourceType.SupplyResource)
            {
                var playerMaxSupply = MaxSupplyResourcesAmount
                    .FirstOrDefault(x => x.Resource == gameResource);

                return playerMaxSupply != null && playerResource.Amount + requiredAmount <= playerMaxSupply.Amount;
            }
            return true;
        });
    }

    /// <summary>
    /// Универсальный метод для валидации ресурсов на основе переданной логики проверки.
    /// </summary>
    private bool ValidateResources(ResourceAmount[] resourceAmounts, Func<ResourceAmount, ResourceDefinition, int, bool> validationLogic)
    {
        foreach (var resource in resourceAmounts)
        {
            var gameResource = resource.Resource;

            var playerResource = ResourcesAmount
                .FirstOrDefault(x => x.Resource == resource.Resource);

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
            var gameResource = resource.Resource;
            if (gameResource.Type == ResourceType.SupplyResource)
            {
                continue;
            }

            var playerResource = ResourcesAmount.First(x => x.Resource == resource.Resource);

            var oldValue = playerResource.Amount;
            playerResource.Amount -= resource.Amount;
            var newValue = playerResource.Amount;

            _playerEventController.OnResourceChanged(gameResource, gameResource.Type, oldValue, newValue);
        }
    }
}

[Serializable]
public class ResourceAmount
{
    public ResourceDefinition Resource;

    public int Amount;
}
