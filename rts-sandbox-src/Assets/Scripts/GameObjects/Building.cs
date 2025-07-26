using Assets.Scripts.GameObjects;
using Assets.Scripts.Infrastructure.Constants;
using Assets.Scripts.Infrastructure.Enums;
using Assets.Scripts.Infrastructure.Events;
using UnityEngine;
using UnityEngine.AI;

public class Building : MonoBehaviour
{
    public bool BuildingIsInProgress { get; set; }

    private UnitValues _unitValues;
    private BuildingValues _buildingValues;
    private UnitEventManager _unitEventManager;
    private NavMeshObstacle _navmeshObstacle;

    private UnitProducing _unitProducing;
    private UnitCommandManager _unitCommandManager;
    private HarvestedResourcesStorage _harvestedResourcesStorage;
    private PlayerEventController _playerEventController;
    private PlayerResources _playerResources;

    private float timeToBuild;
    private float hpToBuild;
    private float hpDelta;
    private float currentTimer;

    public void Awake()
    {
        _unitValues = GetComponent<UnitValues>();
        _buildingValues = GetComponent<BuildingValues>();
        _unitEventManager = GetComponent<UnitEventManager>();
        _navmeshObstacle = GetComponent<NavMeshObstacle>();
        _unitProducing = GetComponent<UnitProducing>();
        _unitCommandManager = GetComponent<UnitCommandManager>();
        _harvestedResourcesStorage = GetComponent<HarvestedResourcesStorage>();
        _playerEventController = GameObject.FindGameObjectWithTag(Tag.PlayerController.ToString())
            .GetComponent<PlayerEventController>();
        _playerResources = GameObject.FindGameObjectWithTag(Tag.PlayerController.ToString())
            .GetComponent<PlayerResources>();
        _unitEventManager.Canceled += CancelBulding;
    }

    public void Start()
    {
        var navmeshXSize = _buildingValues.ObstacleSize / transform.lossyScale.x;
        var navmeshZSize = _buildingValues.ObstacleSize / transform.lossyScale.z;
        _navmeshObstacle.size = new Vector3(navmeshXSize, 1f, navmeshZSize);
    }

    public void Build()
    {
        if (_unitProducing != null)
        {
            _unitProducing.enabled = false;
        }

        if (_unitCommandManager != null)
        {
            _unitCommandManager.enabled = false;
        }

        if (_harvestedResourcesStorage != null)
        {
            _harvestedResourcesStorage.enabled = false;
        }

        timeToBuild = _unitValues.ProducingTime;

        _unitValues.CurrentHp = _unitValues.MaximumHp * GameConstants.BuildingHPStartPercent;

        hpToBuild = _unitValues.MaximumHp - _unitValues.CurrentHp;

        hpDelta = hpToBuild / timeToBuild;

        currentTimer = 0f;

        BuildingIsInProgress = true;
    }

    public void Update()
    {
        if (BuildingIsInProgress)
        {
            if (currentTimer < timeToBuild)
            {
                currentTimer += Time.deltaTime;
                _unitValues.CurrentHp += hpDelta * Time.deltaTime;
            }
            else
            {
                CompletBuilding();
            }

            _unitEventManager.OnHealthPointsChanged(_unitValues.CurrentHp);
        }
    }

    protected void CancelBulding(CanceledEventArgs args)
    {
        if (!BuildingIsInProgress)
        {
            return;
        }

        Destroy(gameObject);
        _playerEventController.OnSelectedUnitDied(gameObject);
        _playerEventController.OnBuildingRemoved(gameObject);
        _unitEventManager.OnUnitDied(gameObject, gameObject);

        foreach (var resourceCost in _unitValues.ResourceCost)
        {
            var valueToReturn = Mathf.RoundToInt(resourceCost.Amount * GameConstants.ResourcesReturnedWhenBuildingCanceled);
            _playerResources.AddResource(resourceCost.ResourceName, valueToReturn);
        }
    }

    private void CompletBuilding()
    {
        BuildingIsInProgress = false;
        _unitValues.CurrentHp = Mathf.Round(_unitValues.CurrentHp);

        if (_unitProducing != null)
        {
            _unitProducing.enabled = true;
        }

        if (_unitCommandManager != null)
        {
            _unitCommandManager.enabled = true;
        }

        if (_harvestedResourcesStorage != null)
        {
            _harvestedResourcesStorage.enabled = true;
        }

        _unitEventManager.OnBuildingCompleted(gameObject);
    }
}