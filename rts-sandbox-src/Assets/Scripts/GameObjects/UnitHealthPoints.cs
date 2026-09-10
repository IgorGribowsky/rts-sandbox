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

    public void Awake()
    {
        _unitValues = GetComponent<UnitValues>();
        _buildingValues = GetComponent<BuildingValues>();
        _unitEventManager = GetComponent<UnitEventManager>();
        _playerEventController = GameObject.FindGameObjectWithTag(Tag.PlayerController.ToString())
            .GetComponent<PlayerEventController>();
    }

    private void OnEnable()
    {
        _unitEventManager.DamageReceived += DamageReceivedHandler;
        _hpRegenCoroutine = StartCoroutine(HpRegeneration());
    }

    private void OnDisable()
    {
        _unitEventManager.DamageReceived -= DamageReceivedHandler;

        if (_hpRegenCoroutine != null)
            StopCoroutine(_hpRegenCoroutine);   
    }

    protected void DamageReceivedHandler(DamageReceivedEventArgs args)
    {
        _unitValues.CurrentHp -= args.DamageAmount;

        _unitEventManager.OnHealthPointsChanged(_unitValues.CurrentHp);

        if (_unitValues.CurrentHp <= 0)
        {
            // Everyone is told first and only then is the object destroyed:
            // Destroy runs OnDisable right away, and every subscription made in
            // OnEnable is gone by the next line (T-028).
            _unitEventManager.OnUnitDied(args.Attacker, gameObject);
            _playerEventController.OnSelectedUnitDied(gameObject);

            if (_buildingValues != null)
            {
                _playerEventController.OnBuildingRemoved(gameObject);
            }

            Destroy(gameObject);
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
}
