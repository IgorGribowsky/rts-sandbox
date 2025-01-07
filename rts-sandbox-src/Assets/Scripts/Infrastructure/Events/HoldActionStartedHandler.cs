using System;

namespace Assets.Scripts.Infrastructure.Events
{
    public delegate void HoldActionStartedHandler(HoldActionStartedEventArgs args);

    public class HoldActionStartedEventArgs : EventArgs
    {
        public HoldActionStartedEventArgs()
        {
        }
    }
}
