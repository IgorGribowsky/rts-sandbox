using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The live numbers of one building: its footprint and what it produces.
/// Filled by <see cref="UnitFactory"/> from the type asset; the properties are
/// there so the rest of the code keeps reading it as before.
/// </summary>
public class BuildingValues : MonoBehaviour
{
    [Tooltip("This building's own copy of the type numbers.")]
    public BuildingStats Stats = new BuildingStats();

    public int GridSize { get => Stats.GridSize; set => Stats.GridSize = value; }

    public int ObstacleSize { get => Stats.ObstacleSize; set => Stats.ObstacleSize = value; }

    public bool IsResource { get => Stats.IsResource; set => Stats.IsResource = value; }

    /// <summary>Since T-015 the production list lives here, not on UnitValues.</summary>
    public bool CanProduceUnits { get => Stats.CanProduceUnits; set => Stats.CanProduceUnits = value; }

    public List<UnitTypeData> UnitsToProduce { get => Stats.UnitsToProduce; set => Stats.UnitsToProduce = value; }

    public bool IsHeldMine { get => IsResource && gameObject.GetComponent<ResourceValues>().IsHeldMine; }

    public bool IsMine { get => IsResource && gameObject.GetComponent<ResourceValues>().IsMine; }
}
