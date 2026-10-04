using Assets.Scripts.Infrastructure.Enums;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The live numbers of a worker: what it can mine or cut and how fast. Only a
/// unit that gathers gets this component — a wall used to carry a harvesting
/// rate for no reason (T-015).
/// </summary>
public class HarvestingValues : MonoBehaviour
{
    [Tooltip("This worker's own copy of the type numbers.")]
    public HarvestingStats Stats = new HarvestingStats();

    public bool IsMiner { get => Stats.IsMiner; set => Stats.IsMiner = value; }

    public bool IsHarvestor { get => Stats.IsHarvestor; set => Stats.IsHarvestor = value; }

    public List<ResourceDefinition> HarvestableResources { get => Stats.HarvestableResources; set => Stats.HarvestableResources = value; }

    public float HarvestingRate { get => Stats.HarvestingRate; set => Stats.HarvestingRate = value; }

    public int HarvestingValuePerTick { get => Stats.HarvestingValuePerTick; set => Stats.HarvestingValuePerTick = value; }

    public int HarvestingMaxValue { get => Stats.HarvestingMaxValue; set => Stats.HarvestingMaxValue = value; }
}
