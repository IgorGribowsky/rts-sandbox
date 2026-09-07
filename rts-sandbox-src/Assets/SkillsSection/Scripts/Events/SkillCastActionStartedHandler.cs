using System;
using static Assets.SkillsSection.Scripts.UnitSkills;

namespace Assets.SkillsSection.Scripts.Events
{
    public delegate void SkillCastActionStartedHandler(SkillCastActionStartedEventArgs args);

    public class SkillCastActionStartedEventArgs : EventArgs
    {
        public SkillCastActionStartedEventArgs(UnitSkill unitSkill)
        {
            UnitSkill = unitSkill;
        }

        public UnitSkill UnitSkill { get; set; }
    }
}
