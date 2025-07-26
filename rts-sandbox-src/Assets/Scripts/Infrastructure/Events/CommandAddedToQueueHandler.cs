using Assets.Scripts.Infrastructure.Abstractions;
using System;

namespace Assets.Scripts.Infrastructure.Events
{
    public delegate void CommandAddedToQueueHandler(CommandAddedToQueueEventArgs args);

    public class CommandAddedToQueueEventArgs : EventArgs
    {
        public CommandAddedToQueueEventArgs(ICommand command)
        {
            Command = command;
        }

        public ICommand Command { get; set; }
    }
}
