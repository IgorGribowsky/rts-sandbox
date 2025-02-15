using Assets.Scripts.Infrastructure.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerResources : MonoBehaviour
{
    public List<ResourceAmount> ResourcesAmount = new List<ResourceAmount>();

    void Start()
    {

    }

    void Update()
    {

    }

    public void AddResource(ResourceName resourceName, int amount)
    {
        ResourcesAmount.FirstOrDefault(x => x.ResourceName == resourceName).Amount += amount;
    }

    public void RemoveResource(ResourceName resourceName, int amount)
    {
        ResourcesAmount.FirstOrDefault(x => x.ResourceName == resourceName).Amount -= amount;
    }

    public bool CheckIfCanSpendResources(params ResourceAmount[] resourceAmounts)
    {
        foreach (var resource in resourceAmounts)
        {
            var playerResource = ResourcesAmount.First(x => x.ResourceName == resource.ResourceName);

            if (playerResource == null)
            {
                return false;
            }

            if (playerResource.Amount < resource.Amount)
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
            var playerResource = ResourcesAmount.First(x => x.ResourceName == resource.ResourceName);

            playerResource.Amount -= resource.Amount;
        }
    }
}

[Serializable]
public class ResourceAmount
{
    public ResourceName ResourceName;

    public int Amount;
}
