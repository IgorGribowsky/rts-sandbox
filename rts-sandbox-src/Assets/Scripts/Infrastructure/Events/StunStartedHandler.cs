using System;

namespace Assets.Scripts.Infrastructure.Events
{
    public delegate void StunStartedHandler(StunStartedEventArgs args);

    public class StunStartedEventArgs : EventArgs
    {
        public StunStartedEventArgs()
        {
        }
    }
}
