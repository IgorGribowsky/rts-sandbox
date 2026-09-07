namespace Assets.Scripts.GameObjects.UnitBehaviour
{
    /// <summary>
    /// The order a behaviour answers to. UnitBehaviourManager subscribes to unit
    /// events once and routes them by this type, so a new behaviour never edits it.
    /// </summary>
    public enum UnitActionType
    {
        Move,
        AMove,
        Follow,
        Hold,
        Attack,
        AutoAttackIdle,
        Build,
        Mine,
        Harvest,
        SkillCast,
        Stun,
    }
}
