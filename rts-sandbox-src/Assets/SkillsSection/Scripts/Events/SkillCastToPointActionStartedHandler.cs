using UnityEngine;
using static Assets.SkillsSection.Scripts.UnitSkills;

namespace Assets.SkillsSection.Scripts.Events
{
    public delegate void SkillCastToPointActionStartedHandler(SkillCastToPointActionStartedEventArgs args);

    public class SkillCastToPointActionStartedEventArgs : SkillCastActionStartedEventArgs
    {
        public SkillCastToPointActionStartedEventArgs(UnitSkill unitSkill, Vector3 point)
            : base(unitSkill)
        {
            Point = point;
        }

        public Vector3 Point { get; set; }
    }
}
