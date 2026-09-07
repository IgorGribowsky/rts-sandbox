/// <summary>
/// An action aimed at a circle on the ground. A special case of the cast at a
/// point and inherited from it on purpose (decision of the user): the aim is the
/// same single point, only the action reaches everyone around it. Because of that
/// no new behaviour is needed — SkillCastingToPointBehaviour takes this order
/// too, its CanHandle asks for CastToPointAction.
///
/// The radius lives in the asset because the aiming hint needs it as well
/// (M-020).
/// </summary>
public abstract class CastToAreaAction : CastToPointAction
{
    public float AreaRadius;
}
