using System;
using UnityEngine;

namespace Assets.Scripts.Infrastructure.Events
{
    public delegate void FollowCommandReceivedHandler(FollowCommandReceivedEventArgs args);

    public class FollowCommandReceivedEventArgs : EventArgs
    {
        public FollowCommandReceivedEventArgs(GameObject target)
        {
            Target = target;
        }

        public GameObject Target { get; set; }
    }
}
