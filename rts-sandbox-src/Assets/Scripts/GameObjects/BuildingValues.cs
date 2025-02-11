using Assets.Scripts.Infrastructure.Enums;
using UnityEngine;

public class BuildingValues : MonoBehaviour
{
    public int GridSize = 4;

    public int ObstacleSize = 4;

    public bool IsMine = false;

    public bool IsHeldMine = false;
    
    public ResourceName ResourceName;

    public bool IsHarvestableResource = false;

    public int ResourcesAmount;
}