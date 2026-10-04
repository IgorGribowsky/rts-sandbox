using Assets.Scripts.Infrastructure.Enums;
using Assets.Scripts.Infrastructure.Helpers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HarvestedResourcesStorage : MonoBehaviour
{
    public List<ResourceDefinition> StoredResources = new List<ResourceDefinition>();

    private PlayerResources _playerResources;
    private PlayerEventController _playerEventController;

    private void Awake()
    {
        var teamMember = gameObject.GetComponent<TeamMember>();
        var _playerController = GameObject.FindGameObjectsWithTag(Tag.PlayerController.ToString())
            .FirstOrDefault(x => x.GetComponent<PlayerTeamMember>().TeamId == teamMember.TeamId);

        _playerResources = _playerController?.GetComponent<PlayerResources>();
        _playerEventController = _playerController?.GetComponent<PlayerEventController>();
    }

    /// <summary>
    /// Hands the load in. Through AddResource, so the number at the top of the
    /// screen changes too; before T-056 the amount was written past it.
    /// </summary>
    public void Store(ResourceDefinition resource, int value)
    {
        if (_playerResources == null)
        {
            return;
        }

        _playerResources.AddResource(resource, value);
        _playerEventController?.OnResourceGained(resource, value, gameObject.GetTopCenter());
    }

    public bool CheckIfCanStore(ResourceDefinition resource)
    {
        return StoredResources.Any(x => x == resource) && _playerResources.ResourcesAmount.Any(x => x.Resource == resource);
    }
}
