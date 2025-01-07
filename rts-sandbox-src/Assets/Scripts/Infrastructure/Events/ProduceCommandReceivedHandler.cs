using System;
using UnityEngine;

namespace Assets.Scripts.Infrastructure.Events
{
    public delegate void ProduceCommandReceivedHandler(ProduceCommandReceivedEventArgs args);

    public class ProduceCommandReceivedEventArgs : EventArgs
    {
        public ProduceCommandReceivedEventArgs(int unitId)
        {
            UnitId = unitId;
        }

        public int UnitId { get; set; }
    }
}
