using Assets.Scripts.Infrastructure.Enums;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HarvestedResourcesStorage : MonoBehaviour
{
    public List<ResourceName> StoredResources;

    private PlayerResources _playerResources;

    private void Awake()
    {
        var teamMember = gameObject.GetComponent<TeamMember>();
        var _playerController = GameObject.FindGameObjectsWithTag(Tag.PlayerController.ToString())
            .FirstOrDefault(x => x.GetComponent<PlayerTeamMember>().TeamId == teamMember.TeamId);

        _playerResources = _playerController?.GetComponent<PlayerResources>();
    }

    public void Store(ResourceName resource, int value)
    {
        _playerResources.ResourcesAmount.First(x => x.ResourceName == resource).Amount += value;
    }

    public bool CheckIfCanStore(ResourceName resource)
    {
        return StoredResources.Any(x => x == resource) && _playerResources.ResourcesAmount.Any(x => x.ResourceName == resource);
    }
}
