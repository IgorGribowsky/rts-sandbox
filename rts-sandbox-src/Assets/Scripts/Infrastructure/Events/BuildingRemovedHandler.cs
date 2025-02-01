using System;
using UnityEngine;

namespace Assets.Scripts.Infrastructure.Events
{
    public delegate void BuildingRemovedHandler(BuildingRemovedEventArgs args);

    public class BuildingRemovedEventArgs : EventArgs
    {
        public BuildingRemovedEventArgs(GameObject building)
        {
            Building = building;
        }

        public GameObject Building { get; set; }
    }
}
