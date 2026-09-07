using System;
using UnityEngine;

namespace Assets.Scripts.Infrastructure.Constants
{
    /// <summary>
    /// Keys the game itself has taken, so a skill is not allowed to sit on them.
    /// The one list: WindowsInputController builds its own set from here, and
    /// UnitSkills validates the setup against it.
    /// </summary>
    public static class ReservedKeys
    {
        public static readonly KeyCode[] All =
        {
            // mouse
            KeyCode.Mouse0,
            KeyCode.Mouse1,
            KeyCode.Mouse2,

            // orders and modes
            KeyCode.A,          // a-move
            KeyCode.H,          // hold
            KeyCode.B,          // building menu
            KeyCode.S,          // reserved for the stop order, T-032
            KeyCode.Escape,     // cancel
            KeyCode.Space,      // camera to the most important unit
            KeyCode.LeftShift,  // add the order to the queue
            KeyCode.F8,         // fix the camera

            // production
            KeyCode.Alpha1,
            KeyCode.Alpha2,
            KeyCode.Alpha3,
            KeyCode.Alpha4,
            KeyCode.Alpha5,
            KeyCode.Alpha6,
            KeyCode.Alpha7,
            KeyCode.Alpha8,
            KeyCode.Alpha9,
            KeyCode.Alpha0,
        };

        public static bool IsReserved(KeyCode key) => Array.IndexOf(All, key) >= 0;
    }
}
