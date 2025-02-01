using Assets.Scripts.Infrastructure.Enums;
using Assets.Scripts.Infrastructure.Events;
using UnityEngine;

public class UnitHealthPoints : MonoBehaviour
{
    private UnitValues _unitValues;
    private BuildingValues _buildingValues;
    private UnitEventManager _unitEventManager;
    private UnitsController _playerUnitController;
    private BuildingController _buildingController;

    public void Update()
    {
    }

    public void Start()
    {
        _unitValues = GetComponent<UnitValues>();
        _buildingValues = GetComponent<BuildingValues>();
        _unitEventManager = GetComponent<UnitEventManager>();

        _unitEventManager.DamageReceived += DamageReceivedHandler;
        _playerUnitController = GameObject.FindGameObjectWithTag(Tag.PlayerController.ToString()).GetComponent<UnitsController>();
        _buildingController = GameObject.FindGameObjectWithTag(Tag.PlayerController.ToString()).GetComponent<BuildingController>();
    }

    protected void DamageReceivedHandler(DamageReceivedEventArgs args)
    {
        _unitValues.CurrentHp -= args.DamageAmount;

        _unitEventManager.OnHealthPointsChanged(_unitValues.CurrentHp);

        if (_unitValues.CurrentHp <= 0)
        {
            Destroy(gameObject);
            _unitEventManager.OnUnitDied(args.Attacker, gameObject);
            _playerUnitController.OnSelectedUnitDied(gameObject);

            if (_buildingValues != null)
            {
                _buildingController.OnBuildingRemoved(gameObject);
            }
        }
    }
}
