using System;

namespace Assets.Scripts.Infrastructure.Events
{
    public delegate void HealthPointsChangedHandler(HealthPointsChangedEventArgs args);

    public class HealthPointsChangedEventArgs : EventArgs
    {
        public HealthPointsChangedEventArgs(float currentHp)
        {
            CurrentHp = currentHp;
        }

        public float CurrentHp { get; set; }
    }
}
