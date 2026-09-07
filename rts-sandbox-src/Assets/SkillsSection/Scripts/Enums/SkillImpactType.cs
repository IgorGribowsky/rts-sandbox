/// <summary>
/// What happens to whoever a skill catches. Same idea as SkillActionType:
/// the impact inside an asset is data, the executor lives outside.
/// </summary>
public enum SkillImpactType
{
    InstantDamage,
    PoisonDamage,
    Stun,
}
