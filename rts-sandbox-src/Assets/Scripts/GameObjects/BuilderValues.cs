using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The live numbers of a builder: what it is able to put down. Only a unit
/// that actually builds gets this component, so asking for it is the same as
/// asking whether the unit is a builder.
/// </summary>
public class BuilderValues : MonoBehaviour
{
    [Tooltip("This builder's own copy of the type numbers.")]
    public BuilderStats Stats = new BuilderStats();

    public bool IsBuilder { get => Stats.IsBuilder; set => Stats.IsBuilder = value; }

    public List<BuildingToProduce> BuildingsToProduce { get => Stats.BuildingsToProduce; set => Stats.BuildingsToProduce = value; }
}
