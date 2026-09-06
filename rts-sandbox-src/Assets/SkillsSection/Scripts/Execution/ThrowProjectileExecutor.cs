using UnityEngine;

public class ThrowProjectileExecutor : CastToPointActionExecutor<ThrowProjectileAction>
{
    public ThrowProjectileExecutor(ThrowProjectileAction data) : base(data) { }

    public override void Act(GameObject owner, Vector3 castPoint)
    {
        var projectile = Object.Instantiate(Data.Projectile.gameObject, owner.transform.position, Quaternion.identity);
        var projectileScript = projectile.GetComponent<ThrownProjectile>();

        projectileScript.StartThrow(CanHit, Hit, owner, DirectionTo(owner, castPoint), Data.ProjectileRange, Data.ProjectileSpeed);
    }

    protected static Vector3 DirectionTo(GameObject owner, Vector3 castPoint)
    {
        var direction = castPoint - owner.transform.position;
        direction.y = 0;
        return direction;
    }

    protected void Hit(GameObject target, GameObject skillOwner)
    {
        SkillImpactExecutorFactory.ApplyUnitImpacts(Data, target, skillOwner);
    }

    protected bool CanHit(GameObject target, GameObject skillOwner)
    {
        return SkillTargetFilter.CanHit(target, skillOwner, Data.TargetType);
    }
}
