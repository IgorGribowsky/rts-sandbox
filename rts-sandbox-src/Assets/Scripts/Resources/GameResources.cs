using Assets.Scripts.Infrastructure.Enums;
using System;
using System.Collections.Generic;
using UnityEngine;

public class GameResources : MonoBehaviour
{
    public List<Resource> Resources;

}

[Serializable]
public class Resource
{
    public ResourceName ResourceName;

    public ResourceType ResourceType;

    [Tooltip("Shown next to the amount in the HUD (M-022).")]
    public Texture2D Icon;
}
