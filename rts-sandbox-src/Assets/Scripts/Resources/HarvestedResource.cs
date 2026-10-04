using Assets.Scripts.Infrastructure.Enums;
using System.Collections.Generic;
using UnityEngine;

public class HarvestedResource : MonoBehaviour
{
    [Tooltip("How many workers cut at this object at once. The rest go to the nearest one with room (T-063).")]
    public int MaxHarvesters = 3;

    private static readonly List<HarvestedResource> _all = new List<HarvestedResource>();

    /// <summary>Every tree standing now. Read only — do not hold on to the list.</summary>
    public static IReadOnlyList<HarvestedResource> All => _all;

    /// <summary>
    /// Goes up whenever a tree comes or goes: the fog of war rebuilds what
    /// blocks sight only then, not every pass (T-071.2).
    /// </summary>
    public static int Version { get; private set; }

    private ResourceValues _resourceValues;
    private PlayerEventController _playerEventController;

    private void OnEnable()
    {
        _all.Add(this);
        Version++;
    }

    private void OnDisable()
    {
        _all.Remove(this);
        Version++;
    }

    private void Awake()
    {
        _resourceValues = gameObject.GetComponent<ResourceValues>();
        _playerEventController = GameObject.FindGameObjectWithTag(Tag.PlayerController.ToString())
            .GetComponent<PlayerEventController>();
    }

    public int Take(int value)
    {
        int taken;
        if (_resourceValues.ResourcesAmount >= value)
        {
            taken = value;
        }
        else
        {
            taken = _resourceValues.ResourcesAmount;
        }

        _resourceValues.ResourcesAmount -= taken;

        if (_resourceValues.ResourcesAmount <= 0)
        {
            _playerEventController.OnBuildingRemoved(gameObject);
            Destroy(gameObject);
        }

        return taken;
    }
}
