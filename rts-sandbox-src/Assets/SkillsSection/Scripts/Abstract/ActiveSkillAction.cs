using UnityEngine;

public abstract class ActiveSkillAction : SkillAction
{
    [Tooltip("Which aiming hint is drawn while the key is held (M-020). A choice of " +
             "the designer, not something read off the action class.")]
    public SkillAimHintType AimHint;

    /// <summary>
    /// How far the action itself reaches, or 0 when it has no limit of its own and
    /// the reach is the skill's CastRange. This is what the aiming hint draws
    /// (M-020, decision of the user, answer in chat 2026-09-07): Blink carries a
    /// CastRange of 50 only because the caster never walks for it, while the jump
    /// itself never goes past MaxRange, and a circle of 50 would lie about that.
    ///
    /// A property and not a field on purpose: the number already lives in the
    /// asset of every action that has one, and copying it into a second field
    /// would only let the two drift apart.
    /// </summary>
    public virtual float MaxRange => 0f;
}
