using System;
using UnityEngine;

namespace Assets.Scripts.Infrastructure.Events
{
    public delegate void BuildingCompletedHandler(BuildingCompletedEventArgs args);

    public class BuildingCompletedEventArgs : EventArgs
    {
        public BuildingCompletedEventArgs(GameObject building)
        {
            Building = building;
        }

        public GameObject Building { get; set; }
    }
}
