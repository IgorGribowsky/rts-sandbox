using Assets.Scripts.Infrastructure.Enums;
using System;
using System.Collections.Generic;
using UnityEngine;

public class GameResources : MonoBehaviour
{
    public List<Resource> Resources;

    void Start()
    {
        
    }

    void Update()
    {
        
    }
}

[Serializable]
public class Resource
{
    public ResourceName ResourceName;

    public ResourceType ResourceType;
}
