using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Infrastructure.Events
{
    public delegate void SelectionChangedHandler(SelectionChangedEventArgs args);

    public class SelectionChangedEventArgs : EventArgs
    {
        public SelectionChangedEventArgs(IReadOnlyList<GameObject> units, GameObject mainUnit, int teamId)
        {
            Units = units;
            MainUnit = mainUnit;
            TeamId = teamId;
        }

        public IReadOnlyList<GameObject> Units { get; }

        /// <summary>The one whose skills and buttons the HUD shows, or null.</summary>
        public GameObject MainUnit { get; }

        public int TeamId { get; }
    }
}
