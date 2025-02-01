using System;

namespace Assets.Scripts.Infrastructure.Events.Common
{
    public class CommandReceivedEventArgs : EventArgs
    {
        public CommandReceivedEventArgs(bool addToCommandsQueue = false)
        {
            AddToCommandsQueue = addToCommandsQueue;
        }

        public bool AddToCommandsQueue { get; set; }
    }
}
