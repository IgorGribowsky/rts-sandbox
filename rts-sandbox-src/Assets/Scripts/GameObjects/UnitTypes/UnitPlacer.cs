using UnityEngine;

/// <summary>
/// A unit put on the map by hand in the editor. Carries no gameplay of its own:
/// it says which type stands here and on whose side, and
/// <see cref="SceneUnitsBootstrapper"/> replaces it with a real unit at start.
///
/// Placing units by hand had to keep working, so the placer keeps its own
/// position, rotation and scale — the unit is built exactly where the marker
/// stands.
/// </summary>
public class UnitPlacer : MonoBehaviour
{
    [Tooltip("What stands here.")]
    public UnitTypeData Type;

    [Tooltip("Whose it is. The same number TeamMember used to carry.")]
    public int TeamId = 1;

    [Tooltip("Only for this one object: gold left in this mine. 0 — take the type's number.")]
    public int ResourcesAmountOverride = 0;

    [Tooltip("Only for this one object: size of the NavMesh obstacle under it. " +
             "0 — take the type's number. Set when a building has to squeeze in.")]
    public int ObstacleSizeOverride = 0;

    /// <summary>Applies what belongs to this marker and not to the type.</summary>
    public void ApplyOverrides(GameObject unit)
    {
        if (ResourcesAmountOverride > 0)
        {
            var resourceValues = unit.GetComponent<ResourceValues>();
            if (resourceValues != null)
            {
                resourceValues.ResourcesAmount = ResourcesAmountOverride;
            }
        }

        if (ObstacleSizeOverride > 0)
        {
            var buildingValues = unit.GetComponent<BuildingValues>();
            if (buildingValues != null)
            {
                buildingValues.ObstacleSize = ObstacleSizeOverride;
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.5f);
        Gizmos.DrawWireCube(transform.position, transform.lossyScale);
    }
}
