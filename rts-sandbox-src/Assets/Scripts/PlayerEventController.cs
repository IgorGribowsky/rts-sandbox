using Assets.Scripts.Infrastructure.Abstractions;
using Assets.Scripts.Infrastructure.Enums;
using Assets.Scripts.Infrastructure.Events;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEventController : MonoBehaviour
{
    public event CursorMovedHandler CursorMoved;
    public void OnCursorMoved(Vector3 cursorPosition, GameObject unitUnderCursor)
    {
        CursorMoved?.Invoke(new CursorMovedEventArgs(cursorPosition, unitUnderCursor));
    }

    public event ModStateChangedHandler BuildingModChanged;
    public void OnBuildingModChanged(bool state)
    {
        BuildingModChanged?.Invoke(new ModStateChangedEventArgs(state));
    }

    public event BuildingStartedHandler BuildingStarted;
    public void OnBuildingStarted(Vector3 point, GameObject builder, GameObject building)
    {
        BuildingStarted?.Invoke(new BuildingStartedEventArgs(point, builder, building));
    }

    public event BuildingRemovedHandler BuildingRemoved;
    public void OnBuildingRemoved(GameObject building)
    {
        BuildingRemoved?.Invoke(new BuildingRemovedEventArgs(building));
    }

    public event ResourceChangedHandler ResourceChanged;
    public void OnResourceChanged(ResourceName name, ResourceType type, int oldValue, int newValue)
    {
        ResourceChanged?.Invoke(new ResourceChangedEventArgs(name, type, oldValue, newValue));
    }

    public event DiedHandler SelectedUnitDied;
    public void OnSelectedUnitDied(GameObject dead)
    {
        SelectedUnitDied?.Invoke(new DiedEventArgs(null, dead));
    }

    public event CurrentCommandEndedHandler CurrentCommandEnded;
    public void OnCurrentCommandEnded(ICommand command)
    {
        CurrentCommandEnded?.Invoke(new CurrentCommandEndedEventArgs(command));
    }

    public event CommandAddedToQueueHandler CommandAddedToQueue;
    public void OnCommandAddedToQueue(ICommand command)
    {
        CommandAddedToQueue?.Invoke(new CommandAddedToQueueEventArgs(command));
    }

    public event CommandsQueueClearedHandler CommandsQueueCleared;
    public void OnCommandsQueueCleared(IEnumerable<ICommand> command)
    {
        CommandsQueueCleared?.Invoke(new CommandsQueueClearedEventArgs(command));
    }

}