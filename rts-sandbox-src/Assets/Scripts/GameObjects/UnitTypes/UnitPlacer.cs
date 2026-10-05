using UnityEngine;

/// <summary>
/// A unit put on the map by hand in the editor. Carries no gameplay of its own:
/// it says which type stands here and on whose side, and
/// <see cref="SceneUnitsBootstrapper"/> replaces it with a real unit at start.
///
/// Placing units by hand had to keep working, so the placer keeps its own
/// position, rotation and scale — the unit is built exactly where the marker
/// stands.
///
/// In the editor its preview shows the TeamColor parts in the colour of its
/// team, the way the unit will look in the game.
/// </summary>
[ExecuteAlways]
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

#if UNITY_EDITOR
    private static readonly int ColorId = Shader.PropertyToID("_Color");

    private void OnEnable()
    {
        PaintPreview();
    }

    private void OnValidate()
    {
        // Not straight away: Unity does not like renderers touched inside OnValidate.
        UnityEditor.EditorApplication.delayCall += PaintPreview;
    }

    /// <summary>
    /// Paints the preview's TeamColor slots in the colour of <see cref="TeamId"/>,
    /// taken from the TeamController of the placer's scene. Done with property
    /// blocks, so neither the shared materials nor the scene change. No such
    /// team — the slots go back to the model's own colour.
    /// </summary>
    public void PaintPreview()
    {
        if (this == null || Application.isPlaying)
        {
            return;
        }

        Team team = null;
        foreach (var controller in FindObjectsByType<TeamController>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (controller.gameObject.scene == gameObject.scene && controller.Teams != null)
            {
                team = controller.Teams.Find(t => t != null && t.Id == TeamId);
                break;
            }
        }

        var block = new MaterialPropertyBlock();
        foreach (var renderer in GetComponentsInChildren<Renderer>(true))
        {
            var materials = renderer.sharedMaterials;
            for (var i = 0; i < materials.Length; i++)
            {
                if (materials[i] == null || materials[i].name != TeamMember.TeamColorMaterialName)
                {
                    continue;
                }

                block.Clear();
                if (team != null)
                {
                    block.SetColor(ColorId, team.Color);
                }

                renderer.SetPropertyBlock(block, i);
            }
        }
    }
#endif

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.5f);
        Gizmos.DrawWireCube(transform.position, transform.lossyScale);
    }
}
