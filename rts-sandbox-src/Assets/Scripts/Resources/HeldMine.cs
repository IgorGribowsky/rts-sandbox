using Assets.Scripts.Infrastructure.Enums;
using Assets.Scripts.Infrastructure.Events;
using UnityEngine;

public class HeldMine : MonoBehaviour
{
    public GameObject ParentMine;

    private BuildingValues _buildingValues;
    private UnitEventManager _unitEventManager;
    private BuildingController _buildingController;

    void Awake()
    {
        _buildingValues = GetComponent<BuildingValues>();
        _unitEventManager = GetComponent<UnitEventManager>();
        _buildingController = GameObject.FindGameObjectWithTag(Tag.PlayerController.ToString())
            .GetComponent<BuildingController>();
    }

    // Start is called before the first frame update
    void Start()
    {
        _unitEventManager.UnitDied += CreateParentMine;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CreateParentMine(DiedEventArgs args)
    {
        var point = gameObject.transform.position;
        var mine = Instantiate(ParentMine, point, gameObject.transform.rotation);
        _buildingController.OnBuildingStarted(point, null, mine);
        var mineBuildingValues = mine.GetComponent<BuildingValues>();

        mineBuildingValues.ResourcesAmount = _buildingValues.ResourcesAmount;
    }
}
