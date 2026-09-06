namespace Assets.Scripts.GameObjects.UnitBehaviour
{
    /// <summary>
    /// The only place that knows which class stands behind which behaviour type.
    /// Adding a behaviour is a new class, a new UnitBehaviourType value and one
    /// line here. An explicit switch and not reflection on purpose: managed code
    /// stripping drops classes nobody references by name.
    /// </summary>
    public static class UnitBehaviourFactory
    {
        public static UnitBehaviourBase Create(UnitBehaviourType type)
        {
            switch (type)
            {
                case UnitBehaviourType.Movement: return new MovementBehaviour();
                case UnitBehaviourType.AMovement: return new AMovementBehaviour();
                case UnitBehaviourType.Following: return new FollowingBehaviour();
                case UnitBehaviourType.Holding: return new HoldingBehaviour();
                case UnitBehaviourType.MeleeAttacking: return new MeleeAttackingBehaviour();
                case UnitBehaviourType.RangeAttacking: return new RangeAttackingBehaviour();
                case UnitBehaviourType.AutoAttackIdle: return new AutoAttackIdleBehaviour();
                case UnitBehaviourType.AutoAttackBuilding: return new AutoAttackBuildingBehaviour();
                case UnitBehaviourType.Building: return new BuildingBehaviour();
                case UnitBehaviourType.Mining: return new MiningBehaviour();
                case UnitBehaviourType.Harvesting: return new HarvestingBehaviour();
                case UnitBehaviourType.SkillCastingToPoint: return new SkillCastingToPointBehaviour();
                default: return null;
            }
        }
    }
}
