using UnityEngine;

public class ThrowWaveProjectileExecutor : CastToPointActionExecutor<ThrowWaveProjectileAction>
{
    public ThrowWaveProjectileExecutor(ThrowWaveProjectileAction data) : base(data) { }

    public override void Act(GameObject owner, Vector3 castPoint)
    {
        var projectile = Object.Instantiate(Data.Projectile, owner.transform.position, Quaternion.identity);
        var projectileScript = projectile.GetComponent<ThrownWaveProjectile>();

        var direction = castPoint - owner.transform.position;
        direction.y = 0;

        projectileScript.StartThrow(CanHit, Hit, owner, direction, Data.ProjectileRange, Data.ProjectileSpeed, Data.WaveWidthScale);
    }

    private void Hit(GameObject target, GameObject skillOwner)
    {
        SkillImpactExecutorFactory.ApplyUnitImpacts(Data, target, skillOwner);
    }

    private bool CanHit(GameObject target, GameObject skillOwner)
    {
        return SkillTargetFilter.CanHit(target, skillOwner, Data.TargetType);
    }
}
