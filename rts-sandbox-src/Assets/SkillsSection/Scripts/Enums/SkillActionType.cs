/// <summary>
/// What kind of action a skill asset describes. The asset carries data only;
/// this tag is what the factory turns into an executor class.
/// </summary>
public enum SkillActionType
{
    ThrowProjectile,
    ThrowWaveProjectile,
    InstantMovementToPoint,
    ApplyImpactsOnDamageDealt,
}
