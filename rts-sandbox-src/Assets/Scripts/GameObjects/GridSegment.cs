using Assets.Scripts.Infrastructure.Enums;
using Assets.Scripts.Infrastructure.Events;
using UnityEngine;

/// <summary>
/// One cell of the build grid (M-010). The cell's box collider is what the
/// placement test hits; what is drawn is a flat picture in a child (T-060),
/// so the look can change without touching the test.
/// </summary>
public class GridSegment : MonoBehaviour
{
    private bool _restricted = false;
    private Renderer _renderer;
    private MeshRenderer _meshRenderer;
    private PlayerEventController _playerEventController;

    public void Awake()
    {
        _meshRenderer = gameObject.GetComponentInChildren<MeshRenderer>(true);
        _renderer = _meshRenderer;

        _playerEventController = GameObject.FindGameObjectWithTag(Tag.PlayerController.ToString())
            .GetComponent<PlayerEventController>();
    }

    private void OnEnable()
    {
        if (_playerEventController == null)
        {
            return;
        }

        _playerEventController.BuildingModChanged += BuildingModChangedHandler;
    }

    private void OnDisable()
    {
        if (_playerEventController == null)
        {
            return;
        }

        _playerEventController.BuildingModChanged -= BuildingModChangedHandler;
    }

    public void ShowOrHideSegment(bool buildingStateEnabled)
    {
        _meshRenderer.enabled = buildingStateEnabled;
    }

    protected void BuildingModChangedHandler(ModStateChangedEventArgs args)
    {
        ShowOrHideSegment(args.State);
    }

    public bool Restricted {
        get { return _restricted; }
        set 
        { 
            _restricted = value;
        }
    }

    private static readonly int ColorId = Shader.PropertyToID("_Color");
    private MaterialPropertyBlock _block;

    /// <summary>
    /// Shared, never copied: hundreds of cells use the same three materials.
    /// A cell that is only background (around every building on the map) is
    /// drawn fainter than the cells under the cursor, through a property block.
    /// </summary>
    public void SetMaterial(Material material, float opacity = 1f)
    {
        if (_renderer.sharedMaterial != material)
        {
            _renderer.sharedMaterial = material;
        }

        if (opacity >= 1f)
        {
            _renderer.SetPropertyBlock(null);
            return;
        }

        _block ??= new MaterialPropertyBlock();
        var color = material.HasProperty(ColorId) ? material.GetColor(ColorId) : Color.white;
        color.a *= opacity;
        _block.SetColor(ColorId, color);
        _renderer.SetPropertyBlock(_block);
    }
}
