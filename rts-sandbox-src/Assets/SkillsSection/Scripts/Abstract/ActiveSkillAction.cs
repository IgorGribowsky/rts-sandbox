using UnityEngine;

public abstract class ActiveSkillAction : SkillAction
{
    [Tooltip("Which aiming hint is drawn while the key is held (M-020). A choice of " +
             "the designer, not something read off the action class.")]
    public SkillAimHintType AimHint;
}
