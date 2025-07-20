using Assets.Scripts.Infrastructure.Constants;
using Assets.Scripts.Infrastructure.Enums;
using Assets.Scripts.Infrastructure.Events;
using System;
using System.Collections.Generic;
using UnityEngine;

public class HeldMine : MonoBehaviour
{
    public GameObject ParentMine;

    public float MiningRate = 1f;

    public int MiningValue = 10;

    public int MinersMaxCount = 5;

    private BuildingValues _buildingValues;
    private Building _buildingScript;
    private UnitEventManager _unitEventManager;
    private BuildingController _buildingController;
    private PlayerResources _playerResources;
    private ResourceValues _resouceValues;

    private List<GameObject> _miners = new List<GameObject>();
    private int?[] _mineCells;

    private float miningProgress = 0f;

    void Awake()
    {
        _buildingValues = GetComponent<BuildingValues>();
        _resouceValues = GetComponent<ResourceValues>();
        _buildingScript = GetComponent<Building>();
        _unitEventManager = GetComponent<UnitEventManager>();
        _buildingController = GameObject.FindGameObjectWithTag(Tag.PlayerController.ToString())
            .GetComponent<BuildingController>();
        _playerResources = GameObject.FindGameObjectWithTag(Tag.PlayerController.ToString())
            .GetComponent<PlayerResources>();
    }

    // Start is called before the first frame update
    void Start()
    {
        _unitEventManager.UnitDied += CreateParentMine;
        _mineCells = new int?[MinersMaxCount];
    }

    // Update is called once per frame
    void Update()
    {
        if (_resouceValues.ResourcesAmount <= 0)
        {
            Destroy(gameObject);
             _buildingController.OnBuildingRemoved(gameObject);
        }

        if (_miners.Count == 0f)
        {
            return;
        }

        var miningSpeed = (MiningRate / MinersMaxCount) * _miners.Count;
        miningProgress += miningSpeed * Time.deltaTime;

        if (miningProgress > MiningRate)
        {
            _playerResources.AddResource(_resouceValues.ResourceName, MiningValue);
            _resouceValues.ResourcesAmount -= MiningValue;
            miningProgress = 0f;
        }
    }

    public bool ChechIfCanAddMiner()
    {
        if (_buildingScript.BuildingIsInProgress)
        {
            return false;
        }

        if (_miners.Count >= MinersMaxCount)
        {
            return false;
        }

        return true;
    }


    public void AddMiner(GameObject miner)
    {
        var n = GetFreeCell();
        if (n == -1)
        {
            return;
        }

        _mineCells[n] = miner.GetInstanceID();

        _miners.Add(miner);
    }

    public void RemoveMiner(GameObject miner)
    {
        var n = GetCellById(miner.GetInstanceID());
        if (n == -1)
        {
            return;
        }

        _mineCells[n] = null;

        _miners.Remove(miner);
    }

    protected void CreateParentMine(DiedEventArgs args)
    {
        var point = gameObject.transform.position;
        var mine = Instantiate(ParentMine, point, gameObject.transform.rotation);
        _buildingController.OnBuildingStarted(point, null, mine);
        var mineResourceValues = mine.GetComponent<ResourceValues>();

        mineResourceValues.ResourcesAmount = _resouceValues.ResourcesAmount;
    }


    public Vector3 GetMiningPoint()
    {
        var n = GetFreeCell();
        if (n == -1)
        {
            return default;
        }

        float R = (_buildingValues.ObstacleSize * Mathf.Sqrt(2)) / 2 + GameConstants.ExtraRadiusForMining;
        float angle = (2 * Mathf.PI * n) / MinersMaxCount;

        float x = gameObject.transform.position.x + R * Mathf.Sin(angle);
        float z = gameObject.transform.position.z - R * Mathf.Cos(angle);

        return new Vector3(x, 0, z);
    }

    private int GetFreeCell()
    {
        var n = 0;
        while (n < MinersMaxCount)
        {
            if (_mineCells[n] == null)
            {
                break;
            }
            else
            {
                n++;
            }
        }

        if (n >= MinersMaxCount)
        {
            return -1;
        }

        return n;
    }

    private int GetCellById(int gameObjectId)
    {
        var n = 0;
        while (n < MinersMaxCount)
        {
            if (_mineCells[n] == gameObjectId)
            {
                break;
            }
            else
            {
                n++;
            }
        }

        if (n >= MinersMaxCount)
        {
            return -1;
        }

        return n;
    }
}
