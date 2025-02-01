using Assets.Scripts.Infrastructure.Events.Common;
using UnityEngine;

namespace Assets.Scripts.Infrastructure.Events
{
    public delegate void BuildCommandReceivedHandler(BuildCommandReceivedEventArgs args);

    public class BuildCommandReceivedEventArgs : CommandReceivedEventArgs
    {
        public BuildCommandReceivedEventArgs(Vector3 point, GameObject building, bool addToCommandsQueue = false)
            : base(addToCommandsQueue)
        {
            Building = building;
            Point = point;
        }

        public Vector3 Point { get; set; }

        public GameObject Building { get; set; }
    }
}
