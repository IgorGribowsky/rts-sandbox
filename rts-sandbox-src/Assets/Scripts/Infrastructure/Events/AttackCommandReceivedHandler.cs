using System;
using UnityEngine;

namespace Assets.Scripts.Infrastructure.Events
{
    public delegate void AttackCommandReceivedHandler(AttackCommandReceivedEventArgs args);

    public class AttackCommandReceivedEventArgs : EventArgs
    {
        public AttackCommandReceivedEventArgs(GameObject target)
        {
            Target = target;
        }

        public GameObject Target { get; set; }
    }
}
