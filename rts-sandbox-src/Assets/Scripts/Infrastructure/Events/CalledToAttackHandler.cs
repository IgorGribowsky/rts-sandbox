using System;
using UnityEngine;

namespace Assets.Scripts.Infrastructure.Events
{
    public delegate void CalledToAttackHandler(CalledToAttackEventArgs args);

    public class CalledToAttackEventArgs : EventArgs
    {
        public CalledToAttackEventArgs(GameObject caller, GameObject target)
        {
            Caller = caller;

            Target = target;
        }

        public GameObject Caller { get; set; }

        public GameObject Target { get; set; }
    }
}
