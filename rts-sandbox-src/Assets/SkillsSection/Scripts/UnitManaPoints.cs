using Assets.Scripts.Infrastructure.Constants;
using Assets.Scripts.Infrastructure.Events;
using System.Collections;
using UnityEngine;

public class UnitManaPoints : MonoBehaviour
{
    private ManaValues _manaValues;
    private UnitEventManager _unitEventManager;

    private Coroutine _manaRegenCoroutine;

    public void Update()
    {
    }

    public void Awake()
    {
        _manaValues = GetComponent<ManaValues>();
        _unitEventManager = GetComponent<UnitEventManager>();
    }

    private void OnEnable()
    {
        _unitEventManager.ManaUsed += ManaUsedHandler;
        _manaRegenCoroutine = StartCoroutine(ManaRegeneration());
    }

    private void OnDisable()
    {
        _unitEventManager.ManaUsed -= ManaUsedHandler;

        if (_manaRegenCoroutine != null)
            StopCoroutine(_manaRegenCoroutine);
    }

    protected void ManaUsedHandler(ManaUsedEventArgs args)
    {
        _manaValues.CurrentMana -= args.ManaUsed;

        _unitEventManager.OnManaPointsChanged(_manaValues.CurrentMana);
    }

    private IEnumerator ManaRegeneration()
    {
        while (true)
        {
            yield return new WaitForSeconds(GameConstants.ManaRegenRate);

            var manaRegenValue = _manaValues.BaseManaRegen;
            if (_manaValues.CurrentMana < _manaValues.MaximumMana && manaRegenValue > 0)
            {
                _manaValues.CurrentMana = Mathf.Min(_manaValues.CurrentMana + manaRegenValue, _manaValues.MaximumMana);

                _unitEventManager.OnManaPointsChanged(_manaValues.CurrentMana);
            }
        }
    }
}
