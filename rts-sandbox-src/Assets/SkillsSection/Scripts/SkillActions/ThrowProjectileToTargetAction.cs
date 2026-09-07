using UnityEngine;

/// <summary>
/// An action with a unit for a target that hands the impacts over to a projectile:
/// the cast finishes, the projectile flies after the target, and only on arrival
/// do the impacts land. No ProjectileRange here on purpose — the projectile
/// follows its target however far it goes.
/// </summary>
[CreateAssetMenu(fileName = "NewThrowProjectileToTargetAction", menuName = "Game/SkillActions/Throw Projectile To Target Action")]
public class ThrowProjectileToTargetAction : CastToTargetAction
{
    public float ProjectileSpeed;

    public ThrownTargetedProjectile Projectile;

    [SerializeField]
    public TargetType _targetType = TargetType.Enemies;
    public override TargetType TargetType => _targetType;

    public override SkillActionType Type => SkillActionType.ThrowProjectileToTarget;
}
