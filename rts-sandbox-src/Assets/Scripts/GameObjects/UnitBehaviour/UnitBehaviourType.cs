namespace Assets.Scripts.GameObjects.UnitBehaviour
{
    /// <summary>
    /// What a unit is able to do. Serialized as a list on UnitBehaviourManager,
    /// so the numbers are explicit and must never be reused for another behaviour.
    /// </summary>
    public enum UnitBehaviourType
    {
        Movement = 0,
        AMovement = 1,
        Following = 2,
        Holding = 3,
        MeleeAttacking = 4,
        RangeAttacking = 5,
        AutoAttackIdle = 6,
        AutoAttackBuilding = 7,
        Building = 8,
        Mining = 9,
        Harvesting = 10,
        SkillCastingToPoint = 11,
        SkillCastingToTarget = 12,
    }
}
