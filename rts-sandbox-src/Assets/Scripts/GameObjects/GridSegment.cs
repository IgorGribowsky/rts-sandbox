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

    public void SetMaterial(Material material)
    {
        _renderer.material = material;
    }
}
