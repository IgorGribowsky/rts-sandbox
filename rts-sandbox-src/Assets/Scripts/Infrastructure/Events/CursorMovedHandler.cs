using System;
using UnityEngine;

namespace Assets.Scripts.Infrastructure.Events
{
    public delegate void CursorMovedHandler(CursorMovedEventArgs args);

    public class CursorMovedEventArgs : EventArgs
    {
        public CursorMovedEventArgs(Vector3 cursorPosition, GameObject unitUnderCursor)
        {
            CursorPosition = cursorPosition;
            UnitUnderCursor = unitUnderCursor;
        }

        public Vector3 CursorPosition { get; set; }

        public GameObject UnitUnderCursor { get; set; }
    }
}
