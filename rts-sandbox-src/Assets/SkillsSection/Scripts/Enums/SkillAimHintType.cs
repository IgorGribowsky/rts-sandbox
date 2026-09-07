/// <summary>
/// Which aiming hint is drawn while the skill key is held (M-020). Picked in
/// the action asset by hand and NOT derived from the action class on purpose:
/// two actions of the same class may want to be aimed differently.
/// </summary>
public enum SkillAimHintType
{
    /// <summary>Nothing but the range circle.</summary>
    None,

    /// <summary>An arrow from the caster to the cursor.</summary>
    Projectile,

    /// <summary>A cross at the cursor.</summary>
    Teleport,

    /// <summary>A cross snapped to the centre of the unit under the cursor.</summary>
    TargetUnit,

    /// <summary>A circle around the cursor, of the action's area radius.</summary>
    Area,
}
