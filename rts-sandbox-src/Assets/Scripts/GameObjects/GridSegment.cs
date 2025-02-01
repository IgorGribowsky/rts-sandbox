using Assets.Scripts.Infrastructure.Enums;
using Assets.Scripts.Infrastructure.Events;
using UnityEngine;

public class GridSegment : MonoBehaviour
{
    public Material BuildingAllowedMaterial;
    public Material BuildingRestrictedMaterial;

    private bool _restricted = false;
    private Renderer _renderer;
    private MeshRenderer _meshRenderer;
    private BuildingController _buildingController;

    public void Awake()
    {
        _renderer = gameObject.GetComponent<Renderer>();
        _meshRenderer = gameObject.GetComponent<MeshRenderer>();

        _buildingController = GameObject.FindGameObjectWithTag(Tag.PlayerController.ToString())
            .GetComponent<BuildingController>();

        _buildingController.BuildingModChanged += BuildingModChangedHandler;
        _renderer.material = BuildingAllowedMaterial;
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
            if (_restricted)
            {
                _renderer.material = BuildingRestrictedMaterial;
            }
            else
            {
                _renderer.material = BuildingAllowedMaterial;
            }
        }
    }

    private void OnDestroy()
    {
        _buildingController.BuildingModChanged -= BuildingModChangedHandler;
    }
}
