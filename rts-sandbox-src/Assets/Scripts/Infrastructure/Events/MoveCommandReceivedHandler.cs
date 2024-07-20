using System;
using UnityEngine;

namespace Assets.Scripts.Infrastructure.Events
{
    public delegate void MoveCommandReceivedHandler(MoveCommandReceivedEventArgs args);

    public class MoveCommandReceivedEventArgs : EventArgs
    {
        public MoveCommandReceivedEventArgs(Vector3 movePoint)
        {
            MovePoint = movePoint;
        }

        public Vector3 MovePoint { get; set; }
    }
}
