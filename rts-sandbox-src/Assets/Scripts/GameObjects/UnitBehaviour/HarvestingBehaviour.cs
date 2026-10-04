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
    private HarvestingValues _harvestingValues;
    private TeamMember _teamMember;

    private GameObject _storage = null;
    private HarvestedResourcesStorage _harvestedResourcesStorageScript;

    private GameObject _resource = null;
    private HarvestedResource _harvestedResourceScript;

    private bool _toStorage = false;
    private bool _isHarvesting = false;

    private GameObject _target;

    private bool _resourceChanged;

    private float harvestTimer = 0;

    public override UnitActionType Trigger => UnitActionType.Harvest;

    protected override void OnInitialize()
    {
        _navmeshMovement = gameObject.GetComponent<NavMeshMovement>();
        _unitEventManager = GetComponent<UnitEventManager>();
        _harvestingValues = GetComponent<HarvestingValues>();
        _teamMember = GetComponent<TeamMember>();
        _unitCommandManager = GetComponent<UnitCommandManager>();

        _navmeshMovement.NavMeshMovementArrive += HandleArrival;
    }

    public override void StartAction(EventArgs args)
    {
        EnableTriggerEndEvent();

        var actionArgs = args as HarvestingActionStartedEventArgs;

        if (_storage != actionArgs.Storage)
        {
            _navmeshMovement.ReleasePlaceAt(_storage);
        }

        _storage = actionArgs.Storage;
        _harvestedResourcesStorageScript = _storage?.GetComponent<HarvestedResourcesStorage>();

        SetResource(actionArgs.Resource);

        _navmeshMovement.SetPassThrough(true);

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
        // Cutting starts on arrival, not while the unit is still walking.
        _isHarvesting = false;
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
            // A tree with room first; when every one is full, the nearest anyway.
            var found = FindResourceWithRoom(null)
                ?? gameObject.GetNearestResourceInRadius(GameConstants.ResourceFindDistance, IsSameResource);
            SetResource(found);
        }
        return _resource;
    }

    private GameObject FindResourceWithRoom(GameObject except)
    {
        return gameObject.GetNearestResourceInRadius(GameConstants.ResourceFindDistance, unit =>
            unit != except
            && IsSameResource(unit)
            && ApproachSlots.TakenCountOf(unit) < MaxHarvestersAt(unit));
    }

    private bool IsSameResource(GameObject unit)
    {
        var resourceValues = unit.GetComponent<ResourceValues>();
        return resourceValues != null && resourceValues.ResourceName == CurrentResource;
    }

    private static int MaxHarvestersAt(GameObject resource)
    {
        var harvested = resource.GetComponent<HarvestedResource>();
        return harvested != null ? harvested.MaxHarvesters : 0;
    }

    /// <summary>Changing the tree gives the place at the old one back.</summary>
    private void SetResource(GameObject resource)
    {
        if (_resource != resource)
        {
            _navmeshMovement.ReleasePlaceAt(_resource);
        }

        _resource = resource;
        _harvestedResourceScript = _resource != null ? _resource.GetComponent<HarvestedResource>() : null;
    }

    /// <summary>Off the gathering route: places back, bumping into others again.</summary>
    private void LeaveRoute()
    {
        _navmeshMovement.ReleasePlaceAt(_resource);
        _navmeshMovement.ReleasePlaceAt(_storage);
        _navmeshMovement.SetPassThrough(false);
    }

    protected override void OnDeactivated()
    {
        LeaveRoute();
    }

    private void HandleNoTarget()
    {
        IsActive = false;
        LeaveRoute();
        _navmeshMovement.Stop();
        if (TriggerEndEventFlag)
        {
            _unitEventManager.OnHarvestingActionEnded();
        }
    }

    /// <summary>
    /// Every worker walks to a place of its own (T-063): around the storage as
    /// many as fit, around a tree no more than its MaxHarvesters. A full tree
    /// sends the worker to the nearest one with room; only when there is none
    /// does it push in the old way.
    /// </summary>
    private void MoveToTarget()
    {
        if (_toStorage)
        {
            if (!_navmeshMovement.TryGoToPlaceAt(_target, GameConstants.HarvestingDistance))
            {
                _navmeshMovement.GoToObject(_target, GameConstants.HarvestingDistance);
            }
            return;
        }

        if (_navmeshMovement.TryGoToPlaceAt(_resource, GameConstants.HarvestingDistance, MaxHarvestersAt(_resource)))
        {
            return;
        }

        var other = FindResourceWithRoom(_resource);
        if (other != null)
        {
            SetResource(other);
            _target = other;

            if (_navmeshMovement.TryGoToPlaceAt(other, GameConstants.HarvestingDistance, MaxHarvestersAt(other)))
            {
                return;
            }
        }

        _navmeshMovement.GoToObject(_target, GameConstants.HarvestingDistance);
    }

    protected override void UpdateAction()
    {
        if (_isHarvesting)
        {
            if (CurrentResourceValues >= _harvestingValues.HarvestingMaxValue)
            {
                _isHarvesting = false;
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
    }

    protected void HandleArrival(EventArgs args)
    {
        if (!IsActive)
        {
            return;
        }

        if (_toStorage)
        {
            StoreResources();
            _toStorage = false;

            // Handing in is instant: the place at the storage is free for the next one.
            _navmeshMovement.ReleasePlaceAt(_storage);

            if (_unitCommandManager.HasCommandInQueue)
            {
                HandleNoTarget();
            }
            else
            {
                FindAndGoToTarget();
            }
        }
        else
        {
            _isHarvesting = true;
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
        if (harvestTimer >= _harvestingValues.HarvestingRate)
        {
            var takenResource = _harvestedResourceScript.Take(_harvestingValues.HarvestingValuePerTick);
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

    public override void Dispose()
    {
        _navmeshMovement.NavMeshMovementArrive -= HandleArrival;
    }
}
