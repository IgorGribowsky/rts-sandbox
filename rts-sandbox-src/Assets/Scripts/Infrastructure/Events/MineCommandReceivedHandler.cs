using Assets.Scripts.Infrastructure.Events.Common;
using UnityEngine;

namespace Assets.Scripts.Infrastructure.Events
{
    public delegate void MineCommandReceivedHandler(MineCommandReceivedEventArgs args);

    public class MineCommandReceivedEventArgs : CommandReceivedEventArgs
    {
        public MineCommandReceivedEventArgs(GameObject mine, bool addToCommandsQueue = false)
            : base(addToCommandsQueue)
        {
            Mine = mine;
        }

        public GameObject Mine { get; set; }
    }
}
