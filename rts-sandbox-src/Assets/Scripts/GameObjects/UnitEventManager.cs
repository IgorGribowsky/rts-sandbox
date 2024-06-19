using Assets.Scripts.Infrastructure.Enums;
using Assets.Scripts.Infrastructure.Events;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class UnitEventManager : MonoBehaviour
{
    public event DamageReceivedHandler DamageReceived;

    public void OnDamageReceived(GameObject attacker, float damageAmount, DamageType damageType)
    {
        Debug.Log($"{gameObject} received {damageAmount} damage of {damageType} type from {attacker}");

        DamageReceived?.Invoke(new DamageReceivedEventArgs(attacker, damageAmount, damageType));
    }

    public event UnitDiedHandler UnitDied;

    public void OnUnitDied(GameObject killer)
    {
        if (killer == null)
        {
            Debug.Log($"{gameObject} died");
        }
        else
        {
            Debug.Log($"{gameObject} killed by {killer}");
        }

        UnitDied?.Invoke(new UnitDiedEventArgs(killer));
    }

    public event HealthPointsChangedHandler HealthPointsChanged;

    public void OnHealthPointsChanged(float currentHp)
    {
        HealthPointsChanged?.Invoke(new HealthPointsChangedEventArgs(currentHp));
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


}
