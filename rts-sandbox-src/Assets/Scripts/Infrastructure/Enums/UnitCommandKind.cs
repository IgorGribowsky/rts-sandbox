namespace Assets.Scripts.Infrastructure.Enums
{
    /// <summary>
    /// What a unit is doing by order, as the interface needs to know it: the
    /// HUD lights the button of the order that is running (M-023). Idle is the
    /// empty queue, where the unit only auto-attacks on the spot.
    /// </summary>
    public enum UnitCommandKind
    {
        Idle,
        Move,
        AMove,
        Attack,
        Follow,
        Hold,
        Build,
        Mine,
        Harvest,
        Gather,
        SkillCast,
    }
}
