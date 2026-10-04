using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The resources of this game, in the order the HUD shows them (M-012, T-052).
/// A resource itself is an asset, ResourceDefinition: a new one is a new asset
/// put into this list, no code changes.
/// </summary>
public class GameResources : MonoBehaviour
{
    [Tooltip("Every resource of the game, in the order the HUD shows them (T-052).")]
    public List<ResourceDefinition> Definitions = new List<ResourceDefinition>();
}
