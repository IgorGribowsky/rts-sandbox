/// <summary>
/// An action with nothing to aim at (T-053): it goes off the moment the key or
/// the button is pressed, with no aiming, no walking and no place in the order
/// queue — the unit carries on with what it was doing, as with Berserk in
/// Warcraft III. The skill's CastDuration and CastRange mean nothing here.
/// </summary>
public abstract class CastWithoutTargetAction : ActiveSkillAction
{
}
