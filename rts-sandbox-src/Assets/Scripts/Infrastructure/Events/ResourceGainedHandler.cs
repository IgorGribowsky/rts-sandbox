using Assets.Scripts.Infrastructure.Enums;
using System;
using UnityEngine;

namespace Assets.Scripts.Infrastructure.Events
{
    public delegate void ResourceGainedHandler(ResourceGainedEventArgs args);

    /// <summary>
    /// A resource came into the treasury at a place in the world: wood handed
    /// in at a storage, a portion of gold from a mine (M-013, M-014). Not the
    /// same as ResourceChanged, which also fires for spending and refunds and
    /// knows no place.
    /// </summary>
    public class ResourceGainedEventArgs : EventArgs
    {
        public ResourceGainedEventArgs(ResourceName name, int amount, Vector3 position)
        {
            Name = name;
            Amount = amount;
            Position = position;
        }

        public ResourceName Name { get; set; }

        public int Amount { get; set; }

        /// <summary>World point the income is shown over: the top of the building.</summary>
        public Vector3 Position { get; set; }
    }
}
