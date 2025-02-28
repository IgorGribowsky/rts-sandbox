using Assets.Scripts.GameObjects;
using Assets.Scripts.GameObjects.UnitBehaviour;
using Assets.Scripts.Infrastructure.Constants;
using Assets.Scripts.Infrastructure.Enums;
using Assets.Scripts.Infrastructure.Events;
using Assets.Scripts.Infrastructure.Helpers;
using System;
using UnityEngine;

public class HarvestingBehaviour : UnitBehaviourBase
{
    public ResourceName? CurrentResource = null;
    public int CurrentResourceValues = 0;

    private NavMeshMovement _navmeshMovement;
    private UnitEventManager _unitEventManager;
    private UnitCommandManager _unitCommandManager;
    private UnitValues _unitValues;
    private TeamMember _teamMember;

    private GameObject _storage = null;
    private HarvestedResourcesStorage _harvestedResourcesStorageScript;

    private GameObject _resource = null;
    private HarvestedResource _harvestedResourceScript;

    private bool _toStorage = false;

    private Vector3 _point;
    private GameObject _target;

    private bool _resourceChanged;

    private float harvestTimer = 0;

    public void Awake()
    {
        _navmeshMovement = gameObject.GetComponent<NavMeshMovement>();
        _unitEventManager = GetComponent<UnitEventManager>();
        _unitValues = GetComponent<UnitValues>();
        _teamMember = GetComponent<TeamMember>();
        _unitCommandManager = GetComponent<UnitCommandManager>();
    }

    public override void StartAction(EventArgs args)
    {
        EnableTriggerEndEvent();

        var actionArgs = args as HarvestingActionStartedEventArgs;

        _storage = actionArgs.Storage;
        _harvestedResourcesStorageScript = _storage?.GetComponent<HarvestedResourcesStorage>();

        _resource = actionArgs.Resource;
        _harvestedResourceScript = _resource?.GetComponent<HarvestedResource>();

        _toStorage = actionArgs.ToStorage;

        var newCurrentResource = _resource != null
            ? _resource.GetComponent<ResourceValues>().ResourceName
            : CurrentResource;

        if (CurrentResource != newCurrentResource)
        {
            _resourceChanged = true;
        }

        CurrentResource = newCurrentResource;

        FindAndGoToTarget();
    }

    private void FindAndGoToTarget()
    {
        _target = _toStorage ? FindStorage() : FindResource();

        if (_target == null)
        {
            HandleNoTarget();
        }
        else
        {
            MoveToTarget();
        }
    }

    private GameObject FindStorage()
    {
        if (_storage == null)
        {
            _storage = gameObject.GetNearestUnitInRadius(GameConstants.StorageFindDistance, unit =>
            {
                var teamMember = unit.GetComponent<TeamMember>();
                var storage = unit.GetComponent<HarvestedResourcesStorage>();
                return teamMember != null && _teamMember.TeamId == teamMember.TeamId && storage != null && storage.isActiveAndEnabled;
            });
            _harvestedResourcesStorageScript = _storage?.GetComponent<HarvestedResourcesStorage>();
        }
        return _storage;
    }

    private GameObject FindResource()
    {
        if (_resource == null)
        {
            _resource = gameObject.GetNearestResourceInRadius(GameConstants.ResourceFindDistance, unit =>
            {
                var resourceValues = unit.GetComponent<ResourceValues>();
                return resourceValues.ResourceName == CurrentResource;
            });
            _harvestedResourceScript = _resource?.GetComponent<HarvestedResource>();
        }
        return _resource;
    }

    private void HandleNoTarget()
    {
        IsActive = false;
        _navmeshMovement.Stop();
        if (TriggerEndEventFlag)
        {
            _unitEventManager.OnHarvestingActionEnded();
        }
    }

    private void MoveToTarget()
    {
        _point = gameObject.GetClosestPointToInteract(_target.transform.position, _target.GetComponent<BuildingValues>().ObstacleSize);
        _navmeshMovement.Go(_point);
    }

    protected override void UpdateAction()
    {
        if (gameObject.GetDistanceTo(_point) < GameConstants.HarvestingDistance)
        {
            HandleArrival();
        }
    }

    private void HandleArrival()
    {
        if (_toStorage)
        {
            StoreResources();
            _toStorage = false;

            if (_unitCommandManager.HasCommandInQueue)
            {
                HandleNoTarget();
            }
            else
            {
                FindAndGoToTarget();
            }

        }
        else if (CurrentResourceValues >= _unitValues.HarvestingMaxValue)
        {
            _toStorage = true;
            FindAndGoToTarget();
        }
        else if (_resource == null)
        {
            FindAndGoToTarget();
        }
        else
        {
            HarvestResource();
        }
    }

    private void StoreResources()
    {
        if (CurrentResource != null && CurrentResourceValues != 0)
        {
            _harvestedResourcesStorageScript.Store(CurrentResource.Value, CurrentResourceValues);
            CurrentResourceValues = 0;
        }
    }

    private void HarvestResource()
    {
        harvestTimer += Time.deltaTime;
        if (harvestTimer >= _unitValues.HarvestingRate)
        {
            var takenResource = _harvestedResourceScript.Take(_unitValues.HarvestingValuePerTick);
            if (_resourceChanged)
            {
                CurrentResourceValues = takenResource;
                _resourceChanged = false;
            }
            else
            {
                CurrentResourceValues += takenResource;
            }
            harvestTimer = 0;
        }
    }
}
