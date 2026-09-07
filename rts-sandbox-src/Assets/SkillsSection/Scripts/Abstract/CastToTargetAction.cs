/// <summary>
/// An action aimed at a unit. The caster walks up to CastRange of the target and
/// only then applies the action, so the target is what the whole thing is about
/// and it can die on the way.
///
/// Who the action is allowed to pick is the usual TargetType through
/// ITargetSelected — that is what makes releasing the key over an ally cancel
/// the cast instead of starting it.
/// </summary>
public abstract class CastToTargetAction : ActiveSkillAction, ITargetSelected
{
    public abstract TargetType TargetType { get; }
}
