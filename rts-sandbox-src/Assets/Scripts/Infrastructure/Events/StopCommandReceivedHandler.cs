using System;

namespace Assets.Scripts.Infrastructure.Events
{
    public delegate void StopCommandReceivedHandler(StopCommandReceivedEventArgs args);

    /// <summary>The stop order (T-032). Never queued: it is the end of the queue.</summary>
    public class StopCommandReceivedEventArgs : EventArgs
    {
    }
}
