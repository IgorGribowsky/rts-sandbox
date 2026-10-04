using Assets.Scripts.Infrastructure.Enums;
using UnityEngine;

public class ResourceValues : MonoBehaviour
{
    public bool IsMine = false;

    public bool IsHeldMine = false;

    public bool IsHarvestedResource = false;

    public ResourceDefinition Resource;

    public int ResourcesAmount;
}