using UnityEngine;
using static Assets.SkillsSection.Scripts.UnitSkills;

namespace Assets.SkillsSection.Scripts.Events
{
    public delegate void SkillCastToPointCommandReceivedHandler(SkillCastToPointCommandReceivedEventArgs args);

    public class SkillCastToPointCommandReceivedEventArgs : SkillCastCommandReceivedEventArgs
    {
        public SkillCastToPointCommandReceivedEventArgs(UnitSkill unitSkill, Vector3 point, bool addToCommandsQueue = false)
            : base(unitSkill, addToCommandsQueue)
        {
            Point = point;
        }

        public Vector3 Point { get; set; }

        public override SkillCastActionStartedEventArgs ToActionArgs()
            => new SkillCastToPointActionStartedEventArgs(UnitSkill, Point);
    }
}
