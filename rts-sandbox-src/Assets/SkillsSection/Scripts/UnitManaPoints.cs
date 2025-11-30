using Assets.Scripts.Infrastructure.Constants;
using Assets.Scripts.Infrastructure.Events;
using System.Collections;
using UnityEngine;

public class UnitManaPoints : MonoBehaviour
{
    private UnitValues _unitValues;
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
        _unitValues = GetComponent<UnitValues>();
        _unitEventManager = GetComponent<UnitEventManager>();

        _unitEventManager.ManaUsed += ManaUsedHandler;
    }

    protected void ManaUsedHandler(ManaUsedEventArgs args)
    {
        _unitValues.CurrentMana -= args.ManaUsed;

        _unitEventManager.OnManaPointsChanged(_unitValues.CurrentMana);
    }

    private IEnumerator ManaRegeneration()
    {
        while (true)
        {
            yield return new WaitForSeconds(GameConstants.ManaRegenRate);

            var manaRegenValue = _unitValues.BaseManaRegen;
            if (_unitValues.CurrentMana < _unitValues.MaximumMana && manaRegenValue > 0)
            {
                _unitValues.CurrentMana = Mathf.Min(_unitValues.CurrentMana + manaRegenValue, _unitValues.MaximumMana);

                _unitEventManager.OnManaPointsChanged(_unitValues.CurrentMana);
            }
        }
    }

    private void OnDestroy()
    {
        _unitEventManager.ManaUsed -= ManaUsedHandler;
    }
}
