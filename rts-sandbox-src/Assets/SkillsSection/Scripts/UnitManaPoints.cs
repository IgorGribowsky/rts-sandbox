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

    private void OnEnable()
    {
        _manaRegenCoroutine = StartCoroutine(ManaRegeneration());
    }

    private void OnDisable()
    {
        if (_manaRegenCoroutine != null)
            StopCoroutine(_manaRegenCoroutine);
    }


    public void Start()
    {
        _manaValues = GetComponent<ManaValues>();
        _unitEventManager = GetComponent<UnitEventManager>();

        _unitEventManager.ManaUsed += ManaUsedHandler;
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

    private void OnDestroy()
    {
        // Start may never have run: an object destroyed in the frame it
        // appeared, or one never activated, reaches OnDestroy with this
        // still null.
        if (_unitEventManager == null)
        {
            return;
        }

        _unitEventManager.ManaUsed -= ManaUsedHandler;
    }
}
