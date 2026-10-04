using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// A see-through copy of a building that is not there yet (M-010, T-060): the
/// one under the cursor while it is being placed, and the ones queued with
/// Shift. Drawn with one material for all its parts; the colour comes per
/// ghost through a property block, so a hundred ghosts share one material.
///
/// It has no colliders and no scripts: the cursor's rays go through it and it
/// never takes part in the game.
/// </summary>
public sealed class BuildingGhost
{
    private const int IgnoreRaycastLayer = 2;
    private static readonly int ColorId = Shader.PropertyToID("_Color");

    private readonly GameObject _body;
    private readonly Renderer[] _renderers;
    private readonly MaterialPropertyBlock _block = new MaterialPropertyBlock();
    private readonly float _lift;
    private Color _color = new Color(0f, 0f, 0f, 0f);

    public BuildingGhost(UnitTypeData type, Material material)
    {
        _body = UnitBodyCopy.Create(type, Vector3.zero, type.BodyPrefab.transform.rotation, IgnoreRaycastLayer);
        _body.name = "Building Ghost " + type.name;

        _renderers = _body.GetComponentsInChildren<Renderer>();
        foreach (var renderer in _renderers)
        {
            var materials = new Material[renderer.sharedMaterials.Length];
            for (var i = 0; i < materials.Length; i++)
            {
                materials[i] = material;
            }

            renderer.sharedMaterials = materials;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }

        // Stands where the real one will: BuildingController lifts a building
        // by half its height, a held mine takes the old mine's place as it is.
        _lift = type.IsHeldMine ? 0f : type.BodyPrefab.transform.localScale.y / 2f;
    }

    /// <summary>The point the building is ordered at, on the ground.</summary>
    public void Place(Vector3 point)
    {
        _body.transform.position = point + Vector3.up * _lift;
    }

    public void SetColor(Color color)
    {
        if (color == _color)
        {
            return;
        }

        _color = color;
        _block.SetColor(ColorId, color);
        foreach (var renderer in _renderers)
        {
            renderer.SetPropertyBlock(_block);
        }
    }

    public void Destroy()
    {
        if (_body != null)
        {
            Object.Destroy(_body);
        }
    }
}
