using System;
using UnityEngine;

namespace Assets.Scripts.Infrastructure.Events
{
    public delegate void MoveActionStartedHandler(MoveActionStartedEventArgs args);

    public class MoveActionStartedEventArgs : EventArgs
    {
        public MoveActionStartedEventArgs(Vector3 movePoint)
        {
            MovePoint = movePoint;
        }

        public Vector3 MovePoint { get; set; }
    }
}
