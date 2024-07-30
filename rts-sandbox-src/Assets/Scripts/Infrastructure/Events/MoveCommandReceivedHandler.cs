using Assets.Scripts.Infrastructure.Events.Common;
using System;
using UnityEngine;

namespace Assets.Scripts.Infrastructure.Events
{
    public delegate void MoveCommandReceivedHandler(MoveCommandReceivedEventArgs args);

    public class MoveCommandReceivedEventArgs : CommandReceivedEventArgs
    {
        public MoveCommandReceivedEventArgs(Vector3 movePoint, bool addToCommandsQueue = false)
            : base(addToCommandsQueue)
        {
            MovePoint = movePoint;
        }

        public Vector3 MovePoint { get; set; }
    }
}
