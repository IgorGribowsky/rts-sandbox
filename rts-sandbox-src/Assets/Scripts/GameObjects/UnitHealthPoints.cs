using Assets.Scripts.Infrastructure.Enums;
using Assets.Scripts.Infrastructure.Events;
using UnityEngine;

public class UnitHealthPoints : MonoBehaviour
{
    private UnitValues _unitValues;
    private BuildingValues _buildingValues;
    private UnitEventManager _unitEventManager;
    private PlayerEventController _playerEventController;

    public void Update()
    {
    }

    public void Start()
    {
        _unitValues = GetComponent<UnitValues>();
        _buildingValues = GetComponent<BuildingValues>();
        _unitEventManager = GetComponent<UnitEventManager>();
        _playerEventController = GameObject.FindGameObjectWithTag(Tag.PlayerController.ToString())
            .GetComponent<PlayerEventController>();

        _unitEventManager.DamageReceived += DamageReceivedHandler;
    }

    protected void DamageReceivedHandler(DamageReceivedEventArgs args)
    {
        _unitValues.CurrentHp -= args.DamageAmount;

        _unitEventManager.OnHealthPointsChanged(_unitValues.CurrentHp);

        if (_unitValues.CurrentHp <= 0)
        {
            Destroy(gameObject);
            _unitEventManager.OnUnitDied(args.Attacker, gameObject);
            _playerEventController.OnSelectedUnitDied(gameObject);

            if (_buildingValues != null)
            {
                _playerEventController.OnBuildingRemoved(gameObject);
            }
        }
    }
}
