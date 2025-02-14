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
}

[Serializable]
public class ResourceAmount
{
    public ResourceName ResourceName;

    public int Amount;
}
