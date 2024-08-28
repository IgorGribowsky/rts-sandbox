using System;
using UnityEngine;

namespace Assets.Scripts.Infrastructure.Events
{
    public delegate void ProduceCommandReceivedHandler(ProduceCommandReceivedEventArgs args);

    public class ProduceCommandReceivedEventArgs : EventArgs
    {
        public ProduceCommandReceivedEventArgs(GameObject unit)
        {
            Unit = unit;
        }

        public GameObject Unit { get; set; }
    }
}
