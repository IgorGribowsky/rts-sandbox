using Assets.Scripts.Infrastructure.Enums;
using Assets.Scripts.Infrastructure.Events;
using UnityEngine;

public class GridSegment : MonoBehaviour
{
    private bool _restricted = false;
    private Renderer _renderer;
    private MeshRenderer _meshRenderer;
    private PlayerEventController _playerEventController;

    public void Awake()
    {
        _renderer = gameObject.GetComponent<Renderer>();
        _meshRenderer = gameObject.GetComponent<MeshRenderer>();

        _playerEventController = GameObject.FindGameObjectWithTag(Tag.PlayerController.ToString())
            .GetComponent<PlayerEventController>();

        _playerEventController.BuildingModChanged += BuildingModChangedHandler;
    }

    public void Start()
    {
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

    public void SetMaterial(Material material)
    {
        _renderer.material = material;
    }

    private void OnDestroy()
    {
        // Start may never have run: an object destroyed in the frame it
        // appeared, or one never activated, reaches OnDestroy with this
        // still null.
        if (_playerEventController == null)
        {
            return;
        }

        _playerEventController.BuildingModChanged -= BuildingModChangedHandler;
    }
}
