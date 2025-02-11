using Assets.Scripts.Infrastructure.Enums;
using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerResources : MonoBehaviour
{
    public List<ResourceAmount> ResourcesAmount;

    void Start()
    {
        
    }

    void Update()
    {
        
    }
}

[Serializable]
public class ResourceAmount
{
    public ResourceName ResourceName;

    public int Amount;
}
