using Assets.Scripts.Infrastructure.Enums;
using Assets.Scripts.Infrastructure.Events;
using UnityEngine;

public class UnitEventManager : MonoBehaviour
{
    public event DamageReceivedHandler DamageReceived;

    public void OnDamageReceived(GameObject attacker, float damageAmount, DamageType damageType)
    {
        Debug.Log($"{gameObject} received {damageAmount} damage of {damageType} type from {attacker}");

        DamageReceived?.Invoke(new DamageReceivedEventArgs(attacker, damageAmount, damageType));
    }

    public event DiedHandler UnitDied;

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

        UnitDied?.Invoke(new DiedEventArgs(killer));
    }

    public event HealthPointsChangedHandler HealthPointsChanged;

    public void OnHealthPointsChanged(float currentHp)
    {
        HealthPointsChanged?.Invoke(new HealthPointsChangedEventArgs(currentHp));
    }

    public event MoveCommandReceivedHandler MoveCommandReceived;
    public void OnMoveCommandReceived(Vector3 movePoint)
    {
        MoveCommandReceived?.Invoke(new MoveCommandReceivedEventArgs(movePoint));
    }

    public event AttackCommandReceivedHandler AttackCommandReceived;
    public void OnAttackCommandReceived(GameObject target)
    {
        Debug.Log($"{gameObject} is attacking {target}");

        AttackCommandReceived?.Invoke(new AttackCommandReceivedEventArgs(target));
    }

    public event FollowCommandReceivedHandler FollowCommandReceived;
    public void OnFollowCommandReceived(GameObject target)
    {
        Debug.Log($"{gameObject} is following {target}");

        FollowCommandReceived?.Invoke(new FollowCommandReceivedEventArgs(target));
    }
}
