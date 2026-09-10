using System;
using UnityEngine;

namespace Assets.Scripts.Infrastructure.Events
{
    public delegate void BuildActionStartedHandler(BuildActionStartedEventArgs args);

    public class BuildActionStartedEventArgs : EventArgs
    {
        public BuildActionStartedEventArgs(Vector3 point, UnitTypeData building, bool isMineHeld, GameObject mineToHeld)
        {
            Building = building;
            Point = point;
            IsMineHeld = isMineHeld;
            MineToHeld = mineToHeld;
        }

        public Vector3 Point { get; set; }

        /// <summary>The type being put down, not a prefab: see UnitFactory.</summary>
        public UnitTypeData Building { get; set; }

        public bool IsMineHeld { get; set; }

        public GameObject MineToHeld { get; set; }
    }
}
