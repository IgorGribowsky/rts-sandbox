using System;
using UnityEngine;

namespace Assets.Scripts.Infrastructure.Events
{
    public delegate void HarvestingActionStartedHandler(HarvestingActionStartedEventArgs args);

    public class HarvestingActionStartedEventArgs : EventArgs
    {
        public HarvestingActionStartedEventArgs(GameObject resource, GameObject storage, bool toStorage)
        {
            Resource = resource;
            Storage = storage;
            ToStorage = toStorage;
        }

        public GameObject Resource { get; set; }

        public GameObject Storage { get; set; }

        public bool ToStorage { get; set; }
    }
}
