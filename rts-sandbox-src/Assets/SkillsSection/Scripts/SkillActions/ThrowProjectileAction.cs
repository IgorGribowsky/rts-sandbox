using UnityEngine;

[CreateAssetMenu(fileName = "NewThrowProjectileAction", menuName = "Game/SkillActions/Throw Projectile Action")]
public class ThrowProjectileAction : CastToPointAction, ITargetSelected
{
    public float ProjectileSpeed;

    public float ProjectileRange;

    public ThrownProjectile Projectile;

    [SerializeField]
    public TargetType _targetType = TargetType.Enemies;
    public TargetType TargetType => _targetType;

    /// <summary>The projectile flies from the caster and no further than this.</summary>
    public override float MaxRange => ProjectileRange;

    public override SkillActionType Type => SkillActionType.ThrowProjectile;
}
