using System;
using UnityEngine;

namespace Assets.Scripts.Infrastructure.Events
{
    public delegate void FollowActionStartedHandler(FollowActionStartedEventArgs args);

    public class FollowActionStartedEventArgs : EventArgs
    {
        public FollowActionStartedEventArgs(GameObject target)
        {
            Target = target;
        }

        public GameObject Target { get; set; }
    }
}
