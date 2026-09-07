using UnityEngine;
using static Assets.SkillsSection.Scripts.UnitSkills;

namespace Assets.SkillsSection.Scripts.Events
{
    public delegate void SkillCastToTargetActionStartedHandler(SkillCastToTargetActionStartedEventArgs args);

    public class SkillCastToTargetActionStartedEventArgs : SkillCastActionStartedEventArgs
    {
        public SkillCastToTargetActionStartedEventArgs(UnitSkill unitSkill, GameObject target)
            : base(unitSkill)
        {
            Target = target;
        }

        public GameObject Target { get; set; }
    }
}
