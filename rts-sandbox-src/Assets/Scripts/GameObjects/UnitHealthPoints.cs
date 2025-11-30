using Assets.Scripts.Infrastructure.Constants;
using Assets.Scripts.Infrastructure.Enums;
using Assets.Scripts.Infrastructure.Events;
using System.Collections;
using UnityEngine;

public class UnitHealthPoints : MonoBehaviour
{
    private UnitValues _unitValues;
    private BuildingValues _buildingValues;
    private UnitEventManager _unitEventManager;
    private PlayerEventController _playerEventController;

    private Coroutine _hpRegenCoroutine;

    public void Update()
    {
    }

    private void OnEnable()
    {
        _hpRegenCoroutine = StartCoroutine(HpRegeneration());
    }

    private void OnDisable()
    {
        if (_hpRegenCoroutine != null)
            StopCoroutine(_hpRegenCoroutine);   
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

    private IEnumerator HpRegeneration()
    {
        while (true)
        {
            yield return new WaitForSeconds(GameConstants.HpRegenRate);

            var hpRegenValue = _unitValues.BaseHpRegen;
            if (_unitValues.CurrentHp < _unitValues.MaximumHp && hpRegenValue > 0)
            {
                _unitValues.CurrentHp = Mathf.Min(_unitValues.CurrentHp + hpRegenValue, _unitValues.MaximumHp);

                _unitEventManager.OnHealthPointsChanged(_unitValues.CurrentHp);
            }
        }
    }

    private void OnDestroy()
    {
        _unitEventManager.DamageReceived -= DamageReceivedHandler;
    }
}
