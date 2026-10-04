using System;
using UnityEngine;

namespace Assets.Scripts.Infrastructure.Events
{
    public delegate void CriticalHitHandler(CriticalHitEventArgs args);

    /// <summary>A crit landed (T-070): the HUD floats red numbers of the damage over the attacker.</summary>
    public class CriticalHitEventArgs : EventArgs
    {
        public CriticalHitEventArgs(GameObject attacker, float damage, Vector3 position)
        {
            Attacker = attacker;
            Damage = damage;
            Position = position;
        }

        public GameObject Attacker { get; }

        public float Damage { get; }

        /// <summary>Over the attacker's head at the moment of the hit.</summary>
        public Vector3 Position { get; }
    }
}
