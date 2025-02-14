using System;
using UnityEngine;

namespace Assets.Scripts.Infrastructure.Events
{
    public delegate void MineActionStartedHandler(MineActionStartedEventArgs args);

    public class MineActionStartedEventArgs : EventArgs
    {
        public MineActionStartedEventArgs(GameObject mine)
        {
            Mine = mine;
        }

        public GameObject Mine { get; set; }
    }
}
