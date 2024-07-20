using System;
using UnityEngine;

namespace Assets.Scripts.Infrastructure.Events
{
    public delegate void DiedHandler(DiedEventArgs args);

    public class DiedEventArgs : EventArgs
    {
        public DiedEventArgs(GameObject killer)
        {
            Killer = killer;
        }

        public GameObject Killer { get; set; }
    }
}
