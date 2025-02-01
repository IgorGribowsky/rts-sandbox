using System;
using UnityEngine;

namespace Assets.Scripts.Infrastructure.Events
{
    public delegate void ModStateChangedHandler(ModStateChangedEventArgs args);

    public class ModStateChangedEventArgs : EventArgs
    {
        public ModStateChangedEventArgs(bool state)
        {
            State = state;
        }

        public bool State { get; set; }
    }
}
