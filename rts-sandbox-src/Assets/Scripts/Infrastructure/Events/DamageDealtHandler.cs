using Assets.Scripts.Infrastructure.Enums;
using System;
using UnityEngine;

namespace Assets.Scripts.Infrastructure.Events
{
    public delegate void DamageDealtHandler(DamageDealtEventArgs args);

    /// <summary>
    /// The other side of DamageReceived: this unit has just hit somebody with an
    /// ordinary attack. Raised by whatever lands the attack, not by the damage
    /// event itself, so damage from poison and from skills does not count as an
    /// attack and cannot set an on-hit passive off in a loop.
    /// </summary>
    public class DamageDealtEventArgs : EventArgs
    {
        public DamageDealtEventArgs(GameObject victim, float damageAmount, DamageType damageType)
        {
            Victim = victim;
            DamageAmount = damageAmount;
            DamageType = damageType;
        }

        public GameObject Victim { get; set; }
        public float DamageAmount { get; set; }
        public DamageType DamageType { get; set; }
    }
}
