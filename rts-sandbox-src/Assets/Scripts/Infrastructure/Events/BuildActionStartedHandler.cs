using System;
using UnityEngine;

namespace Assets.Scripts.Infrastructure.Events
{
    public delegate void BuildActionStartedHandler(BuildActionStartedEventArgs args);

    public class BuildActionStartedEventArgs : EventArgs
    {
        public BuildActionStartedEventArgs(Vector3 point, GameObject building)
        {
            Building = building;
            Point = point;
        }

        public Vector3 Point { get; set; }

        public GameObject Building { get; set; }
    }
}
