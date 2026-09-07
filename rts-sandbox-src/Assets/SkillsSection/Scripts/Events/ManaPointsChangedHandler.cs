using System;

namespace Assets.SkillsSection.Scripts.Events
{
    public delegate void ManaPointsChangedHandler(ManaPointsChangedEventArgs args);

    public class ManaPointsChangedEventArgs : EventArgs
    {
        public ManaPointsChangedEventArgs(float currentMana)
        {
            CurrentMana = currentMana;
        }

        public float CurrentMana { get; set; }
    }
}
