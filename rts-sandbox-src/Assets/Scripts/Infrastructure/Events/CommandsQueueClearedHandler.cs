using Assets.Scripts.Infrastructure.Abstractions;
using System;
using System.Collections.Generic;

namespace Assets.Scripts.Infrastructure.Events
{
    public delegate void CommandsQueueClearedHandler(CommandsQueueClearedEventArgs args);

    public class CommandsQueueClearedEventArgs : EventArgs
    {
        public CommandsQueueClearedEventArgs(IEnumerable<ICommand> commands)
        {
            Commands = commands;
        }

        public IEnumerable<ICommand> Commands { get; set; }
    }
}
