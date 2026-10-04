using Assets.Scripts.Infrastructure.Events.Common;

namespace Assets.Scripts.Infrastructure.Events
{
    public delegate void GatherCommandReceivedHandler(GatherCommandReceivedEventArgs args);

    /// <summary>"Gather": the worker picks the target itself when the order starts (M-023).</summary>
    public class GatherCommandReceivedEventArgs : CommandReceivedEventArgs
    {
        public GatherCommandReceivedEventArgs(bool addToCommandsQueue = false)
            : base(addToCommandsQueue)
        {
        }
    }
}
