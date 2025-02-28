using Assets.Scripts.Infrastructure.Enums;
using Assets.Scripts.Infrastructure.Events;
using System;
using UnityEditor.SceneManagement;
using UnityEngine;

public class UnitEventManager : MonoBehaviour
{
    public event DamageReceivedHandler DamageReceived;

    public void OnDamageReceived(GameObject attacker, float damageAmount, DamageType damageType)
    {
        DamageReceived?.Invoke(new DamageReceivedEventArgs(attacker, damageAmount, damageType));
    }

    public event DiedHandler UnitDied;

    public void OnUnitDied(GameObject killer, GameObject dead)
    {
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
        FollowActionStarted?.Invoke(new FollowActionStartedEventArgs(target));
    }

    public event FollowActionEndedHandler FollowActionEnded;
    public void OnFollowActionEnded()
    {
        FollowActionEnded?.Invoke(new EventArgs());
    }

    public event BuildCommandReceivedHandler BuildCommandReceived;
    public void OnBuildCommandReceived(Vector3 point, GameObject building, bool isMineHeld, GameObject mineToHeld, bool addToCommandsQueue = false)
    {
        BuildCommandReceived?.Invoke(new BuildCommandReceivedEventArgs(point, building, isMineHeld, mineToHeld, addToCommandsQueue));
    }

    public event BuildActionStartedHandler BuildActionStarted;
    public void OnBuildActionStarted(Vector3 point, GameObject building, bool isMineHeld, GameObject mineToHeld)
    {
        BuildActionStarted?.Invoke(new BuildActionStartedEventArgs(point, building, isMineHeld, mineToHeld));
    }

    public event BuildingCompletedHandler BuildingCompleted;
    public void OnBuildingCompleted(GameObject building)
    {
        BuildingCompleted?.Invoke(new BuildingCompletedEventArgs(building));
    }

    public event BuildActionEndedHandler BuildActionEnded;
    public void OnBuildActionEnded()
    {
        BuildActionEnded?.Invoke(new EventArgs());
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

    public event CalledToAttackHandler CalledToAttack;
    public void OnCalledToAttack(GameObject caller, GameObject target)
    {
        CalledToAttack?.Invoke(new CalledToAttackEventArgs(caller, target));
    }

    public event MineCommandReceivedHandler MineCommandReceived;
    public void OnMineCommandReceived(GameObject mine, bool addToCommandsQueue = false)
    {
        MineCommandReceived?.Invoke(new MineCommandReceivedEventArgs(mine, addToCommandsQueue));
    }

    public event MineActionStartedHandler MineActionStarted;
    public void OnMineActionStarted(GameObject mine)
    {
        MineActionStarted?.Invoke(new MineActionStartedEventArgs(mine));
    }

    public event MineActionEndedHandler MineActionEnded;
    public void OnMineActionEnded()
    {
        MineActionEnded?.Invoke(new EventArgs());
    }

    public event HarvestingCommandReceivedHandler HarvestingCommandReceived;
    public void OnHarvestingCommandReceived(GameObject resource, GameObject storage, bool toStorage, bool addToCommandsQueue = false)
    {
        HarvestingCommandReceived?.Invoke(new HarvestingCommandReceivedEventArgs(resource, storage, toStorage, addToCommandsQueue));
    }

    public event HarvestingActionStartedHandler HarvestingActionStarted;
    public void OnHarvestingActionStarted(GameObject resource, GameObject storage, bool toStorage)
    {
        HarvestingActionStarted?.Invoke(new HarvestingActionStartedEventArgs(resource, storage, toStorage));
    }

    public event HarvestingActionEndedHandler HarvestingActionEnded;
    public void OnHarvestingActionEnded()
    {
        HarvestingActionEnded?.Invoke(new EventArgs());
    }
}
