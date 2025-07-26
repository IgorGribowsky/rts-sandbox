using System;
using UnityEngine;

namespace Assets.Scripts.Infrastructure.Events
{
    public delegate void CanceledHandler(CanceledEventArgs args);

    public class CanceledEventArgs : EventArgs
    {
        public CanceledEventArgs(GameObject building)
        {
            Building = building;
        }

        public GameObject Building { get; set; }
    }
}
