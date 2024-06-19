using System;
using UnityEngine;

namespace Assets.Scripts.Infrastructure.Events
{
    public delegate void UnitDiedHandler(UnitDiedEventArgs args);

    public class UnitDiedEventArgs : EventArgs
    {
        public UnitDiedEventArgs(GameObject killer)
        {
            Killer = killer;
        }

        public GameObject Killer { get; set; }
    }
}
