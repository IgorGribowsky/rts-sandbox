using Assets.Scripts.Infrastructure.Events.Common;
using UnityEngine;

namespace Assets.Scripts.Infrastructure.Events
{
    public delegate void HarvestingCommandReceivedHandler(HarvestingCommandReceivedEventArgs args);

    public class HarvestingCommandReceivedEventArgs : CommandReceivedEventArgs
    {
        public HarvestingCommandReceivedEventArgs(GameObject resource, GameObject storage, bool toStorage, bool addToCommandsQueue = false)
            : base(addToCommandsQueue)
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
