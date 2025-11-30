using System;

namespace Assets.Scripts.Infrastructure.Events
{
    public delegate void ManaUsedHandler(ManaUsedEventArgs args);

    public class ManaUsedEventArgs : EventArgs
    {
        public ManaUsedEventArgs(float manaUsed)
        {
            ManaUsed = manaUsed;
        }

        public float ManaUsed { get; set; }
    }
}
