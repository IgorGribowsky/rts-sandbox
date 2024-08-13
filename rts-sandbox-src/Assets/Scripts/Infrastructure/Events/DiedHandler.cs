using System;
using UnityEngine;

namespace Assets.Scripts.Infrastructure.Events
{
    public delegate void DiedHandler(DiedEventArgs args);

    public class DiedEventArgs : EventArgs
    {
        public DiedEventArgs(GameObject killer, GameObject dead)
        {
            Killer = killer;
            Dead = dead;
        }

        public GameObject Killer { get; set; }
        public GameObject Dead { get; set; }
    }
}
