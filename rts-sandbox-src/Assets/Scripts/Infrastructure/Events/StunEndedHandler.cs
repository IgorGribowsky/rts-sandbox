using System;

namespace Assets.Scripts.Infrastructure.Events
{
    public delegate void StunEndedHandler(StunEndedEventArgs args);

    public class StunEndedEventArgs : EventArgs
    {
        public StunEndedEventArgs()
        {
        }
    }
}
