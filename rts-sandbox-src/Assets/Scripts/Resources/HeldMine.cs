using Assets.Scripts.Infrastructure.Constants;
using Assets.Scripts.Infrastructure.Enums;
using Assets.Scripts.Infrastructure.Events;
using System;
using System.Collections.Generic;
using UnityEngine;

public class HeldMine : MonoBehaviour
{
    [Tooltip("The plain mine left on the map when this held mine dies.")]
    public UnitTypeData ParentMine;

    public float MiningRate = 1f;

    public int MiningValue = 10;

    public int MinersMaxCount = 5;

    private BuildingValues _buildingValues;
    private Building _buildingScript;
    private UnitEventManager _unitEventManager;
    private PlayerResources _playerResources;
    private ResourceValues _resouceValues;
    private PlayerEventController _playerEventController;

    private List<GameObject> _miners = new List<GameObject>();
    private int?[] _mineCells;

    private float miningProgress = 0f;

    void Awake()
    {
        _buildingValues = GetComponent<BuildingValues>();
        _resouceValues = GetComponent<ResourceValues>();
        _buildingScript = GetComponent<Building>();
        _unitEventManager = GetComponent<UnitEventManager>();
        _playerEventController = GameObject.FindGameObjectWithTag(Tag.PlayerController.ToString())
            .GetComponent<PlayerEventController>();
        _playerResources = GameObject.FindGameObjectWithTag(Tag.PlayerController.ToString())
            .GetComponent<PlayerResources>();

        _mineCells = new int?[MinersMaxCount];
    }

    private void OnEnable()
    {
        _unitEventManager.UnitDied += CreateParentMine;
    }

    private void OnDisable()
    {
        _unitEventManager.UnitDied -= CreateParentMine;
    }

    // Update is called once per frame
    void Update()
    {
        if (_resouceValues.ResourcesAmount <= 0)
        {
            // Destroy last, see UnitHealthPoints: it takes the subscriptions with it.
            _unitEventManager.OnMineIsFinished(gameObject);
            _playerEventController.OnSelectedUnitDied(gameObject);
            _playerEventController.OnBuildingRemoved(gameObject);

            Destroy(gameObject);
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
        var teamId = GetComponent<TeamMember>()?.TeamId ?? 0;
        var leftInMine = _resouceValues.ResourcesAmount;

        // The gold left over is this mine's own, not the type's, so it is set
        // while the new mine is still switched off.
        var mine = UnitFactory.Create(ParentMine, point, gameObject.transform.rotation, teamId,
            created => created.GetComponent<ResourceValues>().ResourcesAmount = leftInMine);

        _playerEventController.OnBuildingStarted(point, null, mine);
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
