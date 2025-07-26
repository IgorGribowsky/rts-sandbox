using Assets.Scripts.Infrastructure.Abstractions;
using System;

namespace Assets.Scripts.Infrastructure.Events
{
    public delegate void CurrentCommandEndedHandler(CurrentCommandEndedEventArgs args);

    public class CurrentCommandEndedEventArgs : EventArgs
    {
        public CurrentCommandEndedEventArgs(ICommand command)
        {
            Command = command;
        }

        public ICommand Command { get; set; }
    }
}
