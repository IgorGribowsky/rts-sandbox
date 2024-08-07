﻿using System;
using UnityEngine;

namespace Assets.Scripts.Infrastructure.Events
{
    public delegate void AttackActionStartedHandler(AttackActionStartedEventArgs args);

    public class AttackActionStartedEventArgs : EventArgs
    {
        public AttackActionStartedEventArgs(GameObject target)
        {
            Target = target;
        }

        public GameObject Target { get; set; }
    }
}
