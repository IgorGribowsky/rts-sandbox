using Assets.Scripts.Infrastructure.Events.Common;

namespace Assets.Scripts.Infrastructure.Events
{
    public delegate void HoldCommandReceivedHandler(HoldCommandReceivedEventArgs args);

    public class HoldCommandReceivedEventArgs : CommandReceivedEventArgs
    {
        public HoldCommandReceivedEventArgs(bool addToCommandsQueue = false)
            : base(addToCommandsQueue)
        {
        }
    }
}
