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

    public void OnUnitDied(GameObject killer, GameObject dead)
    {
        if (killer == null)
        {
            Debug.Log($"{gameObject} died");
        }
        else
        {
            Debug.Log($"{gameObject} killed by {killer}");
        }

        UnitDied?.Invoke(new DiedEventArgs(killer, dead));
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

    public event MoveCommandReceivedHandler AMoveCommandReceived;
    public void OnAMoveCommandReceived(Vector3 movePoint, bool addToCommandsQueue = false)
    {
        AMoveCommandReceived?.Invoke(new MoveCommandReceivedEventArgs(movePoint, addToCommandsQueue));
    }

    public event MoveActionStartedHandler AMoveActionStarted;
    public void OnAMoveActionStarted(Vector3 movePoint)
    {
        AMoveActionStarted?.Invoke(new MoveActionStartedEventArgs(movePoint));
    }

    public event MoveActionEndedHandler AMoveActionEnded;
    public void OnAMoveActionEnded()
    {
        AMoveActionEnded?.Invoke(new EventArgs());
    }

    public event AutoAttackIdleStartedHandler AutoAttackIdleStarted;
    public void OnAutoAttackIdleStarted(Vector3 movePoint)
    {
        AutoAttackIdleStarted?.Invoke(new AutoAttackIdleStartedEventArgs(movePoint));
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

    public event ProduceCommandReceivedHandler ProduceCommandReceived;
    public void OnProduceCommandReceived(int unitId)
    {
        ProduceCommandReceived?.Invoke(new ProduceCommandReceivedEventArgs(unitId));
    }

    public event HoldCommandReceivedHandler HoldCommandReceived;
    public void OnHoldCommandReceived(bool addToCommandsQueue = false)
    {
        HoldCommandReceived?.Invoke(new HoldCommandReceivedEventArgs(addToCommandsQueue));
    }

    public event HoldActionStartedHandler HoldActionStarted;
    public void OnHoldActionStarted()
    {
        HoldActionStarted?.Invoke(new HoldActionStartedEventArgs());
    }
}
