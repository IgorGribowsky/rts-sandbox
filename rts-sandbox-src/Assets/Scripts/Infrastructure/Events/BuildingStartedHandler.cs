using System;
using UnityEngine;

namespace Assets.Scripts.Infrastructure.Events
{
    public delegate void BuildingStartedHandler(BuildingStartedEventArgs args);

    public class BuildingStartedEventArgs : EventArgs
    {
        public BuildingStartedEventArgs(Vector3 point, GameObject builder, GameObject building)
        {
            Building = building;
            Builder = builder;
            Point = point;
        }

        public Vector3 Point { get; set; }

        public GameObject Builder { get; set; }

        public GameObject Building { get; set; }
    }
}
