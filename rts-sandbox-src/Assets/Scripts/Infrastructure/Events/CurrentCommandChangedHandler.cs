using Assets.Scripts.Infrastructure.Abstractions;
using System;

namespace Assets.Scripts.Infrastructure.Events
{
    public delegate void CurrentCommandChangedHandler(CurrentCommandChangedEventArgs args);

    public class CurrentCommandChangedEventArgs : EventArgs
    {
        public CurrentCommandChangedEventArgs(ICommand command)
        {
            Command = command;
        }

        /// <summary>The order now running, or null when the unit went idle.</summary>
        public ICommand Command { get; set; }
    }
}
