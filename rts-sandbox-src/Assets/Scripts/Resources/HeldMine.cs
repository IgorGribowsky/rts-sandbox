using Assets.Scripts.Infrastructure.Constants;
using Assets.Scripts.Infrastructure.Enums;
using Assets.Scripts.Infrastructure.Events;
using Assets.Scripts.Infrastructure.Extensions;
using Assets.Scripts.Infrastructure.Helpers;
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
    private ResourceValues _resourceValues;
    private PlayerEventController _playerEventController;

    private List<GameObject> _miners = new List<GameObject>();
    private int?[] _mineCells;

    // The count the cells are laid out for; differs from MinersMaxCount for one
    // frame after it is changed in the inspector during Play (T-036).
    private int _cellsLaidOutFor;

    private float miningProgress = 0f;

    void Awake()
    {
        _buildingValues = GetComponent<BuildingValues>();
        _resourceValues = GetComponent<ResourceValues>();
        _buildingScript = GetComponent<Building>();
        _unitEventManager = GetComponent<UnitEventManager>();
        _playerEventController = GameObject.FindGameObjectWithTag(Tag.PlayerController.ToString())
            .GetComponent<PlayerEventController>();
        _playerResources = GameObject.FindGameObjectWithTag(Tag.PlayerController.ToString())
            .GetComponent<PlayerResources>();

        _mineCells = new int?[MinersMaxCount];
        _cellsLaidOutFor = MinersMaxCount;
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
        SyncCellsWithMaxCount();

        if (_resourceValues.ResourcesAmount <= 0)
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
            _playerResources.AddResource(_resourceValues.Resource, MiningValue);
            _playerEventController.OnResourceGained(_resourceValues.Resource, MiningValue, gameObject.GetTopCenter());
            _resourceValues.ResourcesAmount -= MiningValue;
            miningProgress = 0f;
        }
    }

    public bool CheckIfCanAddMiner()
    {
        SyncCellsWithMaxCount();

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
        SyncCellsWithMaxCount();

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
        var leftInMine = _resourceValues.ResourcesAmount;

        // The gold left over is this mine's own, not the type's, so it is set
        // while the new mine is still switched off.
        var mine = UnitFactory.Create(ParentMine, point, gameObject.transform.rotation, teamId,
            created => created.GetComponent<ResourceValues>().ResourcesAmount = leftInMine);

        _playerEventController.OnBuildingStarted(point, null, mine);
    }


    public Vector3 GetMiningPoint()
    {
        SyncCellsWithMaxCount();

        var n = GetFreeCell();
        if (n == -1)
        {
            return default;
        }

        return GetCellPoint(n);
    }

    private Vector3 GetCellPoint(int n)
    {
        float R = (_buildingValues.ObstacleSize * Mathf.Sqrt(2)) / 2 + GameConstants.ExtraRadiusForMining;
        float angle = (2 * Mathf.PI * n) / MinersMaxCount;

        float x = gameObject.transform.position.x + R * Mathf.Sin(angle);
        float z = gameObject.transform.position.z - R * Mathf.Cos(angle);

        return new Vector3(x, 0, z);
    }

    /// <summary>
    /// MinersMaxCount changed while playing: the cells grow to match, and the
    /// miners already inside move to the new places around the circle so that
    /// nobody overlaps. Shrinking never drops a cell that has a miner in it —
    /// the array keeps its size, only no new miner is let in above the count.
    /// </summary>
    private void SyncCellsWithMaxCount()
    {
        if (_cellsLaidOutFor == MinersMaxCount || MinersMaxCount <= 0)
        {
            return;
        }

        _mineCells = _mineCells.IncreaseArray(MinersMaxCount - _mineCells.Length);
        _cellsLaidOutFor = MinersMaxCount;

        foreach (var miner in _miners)
        {
            if (miner == null)
            {
                continue;
            }

            var n = GetCellById(miner.GetInstanceID());
            if (n == -1)
            {
                continue;
            }

            var point = GetCellPoint(n);
            miner.transform.position = new Vector3(point.x, miner.transform.position.y, point.z);

            // Same as MiningBehaviour on arrival: the agent's destination goes to
            // the new place too, or it walks the miner straight back.
            var movement = miner.GetComponent<NavMeshMovement>();
            if (movement != null)
            {
                movement.Stop();
            }
        }
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
        while (n < _mineCells.Length)
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

        if (n >= _mineCells.Length)
        {
            return -1;
        }

        return n;
    }

}
