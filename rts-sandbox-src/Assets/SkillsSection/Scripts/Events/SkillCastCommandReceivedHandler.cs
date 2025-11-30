using Assets.Scripts.Infrastructure.Events.Common;
using static Assets.SkillsSection.Scripts.UnitSkills;

namespace Assets.SkillsSection.Scripts.Events
{
    public delegate void SkillCastCommandReceivedHandler(SkillCastCommandReceivedEventArgs args);

    public class SkillCastCommandReceivedEventArgs : CommandReceivedEventArgs
    {
        public SkillCastCommandReceivedEventArgs(UnitSkill unitSkill, bool addToCommandsQueue = false)
            : base(addToCommandsQueue)
        {
            UnitSkill = unitSkill;
        }

        public UnitSkill UnitSkill { get; set; }

        public virtual SkillCastActionStartedEventArgs ToActionArgs()
            => new SkillCastActionStartedEventArgs(UnitSkill);
    }
}
