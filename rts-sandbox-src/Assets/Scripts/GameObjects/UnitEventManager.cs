using Assets.Scripts.Infrastructure.Enums;
using Assets.Scripts.Infrastructure.Events;
using System;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

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
    public void OnMoveCommandReceived(Vector3 movePoint, bool addToCommandsQueue = false)
    {
        MoveCommandReceived?.Invoke(new MoveCommandReceivedEventArgs(movePoint, addToCommandsQueue));
    }

    public event MoveActionStartedHandler MoveActionStarted;
    public void OnMoveActionStarted(Vector3 movePoint)
    {
        MoveActionStarted?.Invoke(new MoveActionStartedEventArgs(movePoint));
    }

    public event MoveActionEndedHandler MoveActionEnded;
    public void OnMoveActionEnded()
    {
        MoveActionEnded?.Invoke(new EventArgs());
    }

    public event AttackCommandReceivedHandler AttackCommandReceived;
    public void OnAttackCommandReceived(GameObject target, bool addToCommandsQueue = false)
    {
        AttackCommandReceived?.Invoke(new AttackCommandReceivedEventArgs(target, addToCommandsQueue));
    }

    public event AttackActionStartedHandler AttackActionStarted;
    public void OnAttackActionStarted(GameObject target)
    {
        Debug.Log($"{gameObject} is attacking {target}");

        AttackActionStarted?.Invoke(new AttackActionStartedEventArgs(target));
    }

    public event AttackActionEndedHandler AttackActionEnded;
    public void OnAttackActionEnded()
    {
        AttackActionEnded?.Invoke(new EventArgs());
    }

    public event FollowCommandReceivedHandler FollowCommandReceived;
    public void OnFollowCommandReceived(GameObject target, bool addToCommandsQueue = false)
    {
        FollowCommandReceived?.Invoke(new FollowCommandReceivedEventArgs(target, addToCommandsQueue));
    }

    public event FollowActionStartedHandler FollowActionStarted;
    public void OnFollowActionStarted(GameObject target)
    {
        Debug.Log($"{gameObject} is following {target}");

        FollowActionStarted?.Invoke(new FollowActionStartedEventArgs(target));
    }

    public event FollowActionEndedHandler FollowActionEnded;
    public void OnFollowActionEnded()
    {
        FollowActionEnded?.Invoke(new EventArgs());
    }
}
