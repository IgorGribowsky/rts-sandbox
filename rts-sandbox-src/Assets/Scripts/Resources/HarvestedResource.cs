using Assets.Scripts.Infrastructure.Enums;
using UnityEngine;

public class HarvestedResource : MonoBehaviour
{
    private BuildingController _buildingController;
    private ResourceValues _resouceValues;

    private void Awake()
    {
        _resouceValues = gameObject.GetComponent<ResourceValues>();
        _buildingController = GameObject.FindGameObjectWithTag(Tag.PlayerController.ToString()).GetComponent<BuildingController>();
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
            _buildingController.OnBuildingRemoved(gameObject);
            Destroy(gameObject);
        }

        return taken;
    }
}
