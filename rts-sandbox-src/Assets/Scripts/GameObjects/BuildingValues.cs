using UnityEngine;

public class BuildingValues : MonoBehaviour
{
    public int GridSize = 4;

    public int ObstacleSize = 4;

    public bool IsResource = false;

    public bool IsHeldMine { get => this.IsResource && gameObject.GetComponent<ResourceValues>().IsHeldMine; }

    public bool IsMine { get => this.IsResource && gameObject.GetComponent<ResourceValues>().IsMine; }
}