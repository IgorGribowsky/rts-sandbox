using Assets.Scripts.Infrastructure.Enums;
using UnityEngine;

public class HarvestedResource : MonoBehaviour
{
    private ResourceValues _resouceValues;
    private PlayerEventController _playerEventController;

    private void Awake()
    {
        _resouceValues = gameObject.GetComponent<ResourceValues>();
        _playerEventController = GameObject.FindGameObjectWithTag(Tag.PlayerController.ToString())
            .GetComponent<PlayerEventController>();
    }

    public int Take(int value)
    {
        int taken;
        if (_resouceValues.ResourcesAmount >= value)
        {
            taken = value;
        }
        else
        {
            taken = _resouceValues.ResourcesAmount;
        }

        _resouceValues.ResourcesAmount -= taken;

        if (_resouceValues.ResourcesAmount <= 0)
        {
            _playerEventController.OnBuildingRemoved(gameObject);
            Destroy(gameObject);
        }

        return taken;
    }
}
