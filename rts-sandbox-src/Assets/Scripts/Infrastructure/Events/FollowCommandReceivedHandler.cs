using Assets.Scripts.Infrastructure.Events.Common;
using UnityEngine;

namespace Assets.Scripts.Infrastructure.Events
{
    public delegate void FollowCommandReceivedHandler(FollowCommandReceivedEventArgs args);

    public class FollowCommandReceivedEventArgs : CommandReceivedEventArgs
    {
        public FollowCommandReceivedEventArgs(GameObject target, bool addToCommandsQueue = false)
            : base(addToCommandsQueue)
        {
            Target = target;
        }

        public GameObject Target { get; set; }
    }
}
