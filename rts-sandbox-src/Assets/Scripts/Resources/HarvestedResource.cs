using Assets.Scripts.Infrastructure.Enums;
using UnityEngine;

public class HarvestedResource : MonoBehaviour
{
    [Tooltip("How many workers cut at this object at once. The rest go to the nearest one with room (T-063).")]
    public int MaxHarvesters = 3;

    private ResourceValues _resourceValues;
    private PlayerEventController _playerEventController;

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
