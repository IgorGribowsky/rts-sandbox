using System;
using UnityEngine;

namespace Assets.Scripts.Infrastructure.Events
{
    public delegate void AutoAttackIdleStartedHandler(AutoAttackIdleStartedEventArgs args);

    public class AutoAttackIdleStartedEventArgs : EventArgs
    {
        public AutoAttackIdleStartedEventArgs(Vector3 movePoint)
        {
            MovePoint = movePoint;
        }

        public Vector3 MovePoint { get; set; }
    }
}
