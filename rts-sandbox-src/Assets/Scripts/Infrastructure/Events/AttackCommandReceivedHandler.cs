using Assets.Scripts.Infrastructure.Events.Common;
using UnityEngine;

namespace Assets.Scripts.Infrastructure.Events
{
    public delegate void AttackCommandReceivedHandler(AttackCommandReceivedEventArgs args);

    public class AttackCommandReceivedEventArgs : CommandReceivedEventArgs
    {
        public AttackCommandReceivedEventArgs(GameObject target, bool addToCommandsQueue = false)
            : base(addToCommandsQueue)
        {
            Target = target;
        }

        public GameObject Target { get; set; }
    }
}
