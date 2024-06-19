using Assets.Scripts.Infrastructure.Enums;
using Assets.Scripts.Infrastructure.Events;
using UnityEngine;

public class UnitHealthPoints : MonoBehaviour
{
    private UnitValues _unitValues;
    private UnitEventManager _unitEventManager;

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            _unitEventManager.OnDamageReceived(gameObject, 25, DamageType.Magic);
        }
    }

    public void Start()
    {
        _unitValues = GetComponent<UnitValues>();
        _unitEventManager = GetComponent<UnitEventManager>();

        _unitEventManager.DamageReceived += DamageReceivedHandler;

        _unitEventManager.UnitDied += UnitDiedHandler;
    }

    protected void DamageReceivedHandler(DamageReceivedEventArgs args)
    {
        _unitValues.CurrentHp -= args.DamageAmount;

        _unitEventManager.OnHealthPointsChanged(_unitValues.CurrentHp);

        if (_unitValues.CurrentHp <= 0)
        {
            _unitEventManager.OnUnitDied(args.Attacker);
        }
    }

    protected void UnitDiedHandler(UnitDiedEventArgs args)
    {
        Destroy(gameObject);
    }
}
