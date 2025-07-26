using Assets.Scripts.Infrastructure.Enums;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public abstract class UnitSupplyBase : MonoBehaviour
{
    protected GameResources _gameResources;
    protected PlayerResources _playerResources;
    protected UnitEventManager _unitEventManager;
    protected UnitValues _unitValues;

    protected virtual void Awake()
    {
        var gameController = GameObject.FindGameObjectWithTag(Tag.GameController.ToString());
        _gameResources = gameController.GetComponent<GameResources>();

        var teamMember = gameObject.GetComponent<TeamMember>();
        var _playerController = GameObject.FindGameObjectsWithTag(Tag.PlayerController.ToString())
            .FirstOrDefault(x => x.GetComponent<PlayerTeamMember>().TeamId == teamMember.TeamId);

        _playerResources = _playerController?.GetComponent<PlayerResources>();
        _unitEventManager = gameObject.GetComponent<UnitEventManager>();
        _unitValues = gameObject.GetComponent<UnitValues>();
    }

    protected abstract void SetupResources();

    protected void ProcessResources(System.Action<ResourceName, int> modifyResource, Func<UnitValues, IEnumerable<ResourceAmount>> resourceSelector)
    {
        if (_unitValues == null || _playerResources == null || _gameResources == null) return;

        var supplyResources = resourceSelector(_unitValues)
            .Where(x => _gameResources.Resources
                .FirstOrDefault(r => r.ResourceName == x.ResourceName)?.ResourceType == ResourceType.SupplyResource);

        foreach (var supplyResource in supplyResources)
        {
            modifyResource(supplyResource.ResourceName, supplyResource.Amount);
        }
    }
}


