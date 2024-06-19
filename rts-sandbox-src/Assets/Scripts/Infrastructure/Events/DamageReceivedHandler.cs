using Assets.Scripts.Infrastructure.Enums;
using System;
using UnityEngine;

namespace Assets.Scripts.Infrastructure.Events
{
    public delegate void DamageReceivedHandler(DamageReceivedEventArgs args);

    public class DamageReceivedEventArgs : EventArgs
    {
        public DamageReceivedEventArgs(GameObject attacker, float damageAmount, DamageType damageType)
        {
            Attacker = attacker;
            DamageAmount = damageAmount;
            DamageType = damageType;
        }

        public GameObject Attacker { get; set; }
        public float DamageAmount { get; set; }
        public DamageType DamageType { get; set; }
    }
}
