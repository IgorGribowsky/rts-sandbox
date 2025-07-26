using System;
using UnityEngine;

namespace Assets.Scripts.Infrastructure.Events
{
    public delegate void MineIsFinishedHandler(MineIsFinishedEventArgs args);

    public class MineIsFinishedEventArgs : EventArgs
    {
        public MineIsFinishedEventArgs(GameObject mine)
        {
            Mine = mine;
        }

        public GameObject Mine { get; set; }
    }
}
