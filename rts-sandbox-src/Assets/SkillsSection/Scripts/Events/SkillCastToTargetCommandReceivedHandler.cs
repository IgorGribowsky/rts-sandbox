using UnityEngine;
using static Assets.SkillsSection.Scripts.UnitSkills;

namespace Assets.SkillsSection.Scripts.Events
{
    public delegate void SkillCastToTargetCommandReceivedHandler(SkillCastToTargetCommandReceivedEventArgs args);

    /// <summary>
    /// A queued cast aimed at a unit. Pair of the "to point" one: the whole order
    /// chain stays as it is, only ToActionArgs tells the behaviours apart.
    /// </summary>
    public class SkillCastToTargetCommandReceivedEventArgs : SkillCastCommandReceivedEventArgs
    {
        public SkillCastToTargetCommandReceivedEventArgs(UnitSkill unitSkill, GameObject target, bool addToCommandsQueue = false)
            : base(unitSkill, addToCommandsQueue)
        {
            Target = target;
        }

        public GameObject Target { get; set; }

        public override SkillCastActionStartedEventArgs ToActionArgs()
            => new SkillCastToTargetActionStartedEventArgs(UnitSkill, Target);
    }
}
