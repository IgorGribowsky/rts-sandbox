using Assets.Scripts.Infrastructure.Constants;
using UnityEngine;
using UnityEngine.AI;

public class Building : MonoBehaviour
{
    public bool BuildingIsInProgress { get; set; }

    private UnitValues _unitValues;
    private BuildingValues _buildingValues;
    private UnitEventManager _unitEventManager;
    private NavMeshObstacle _navmeshObstacle;

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
    }

    public void Start()
    {
        var navmeshXSize = _buildingValues.ObstacleSize / transform.lossyScale.x;
        var navmeshZSize = _buildingValues.ObstacleSize / transform.lossyScale.z;
        _navmeshObstacle.size = new Vector3(navmeshXSize, 1f, navmeshZSize);
    }

    public void Build()
    {
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
                _unitValues.CurrentHp = Mathf.Round(_unitValues.CurrentHp);
                BuildingIsInProgress = false;
            }

            _unitEventManager.OnHealthPointsChanged(_unitValues.CurrentHp);
        }
    }
}