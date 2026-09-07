using UnityEngine;

public class ThrowProjectileToTargetExecutor : CastToTargetActionExecutor<ThrowProjectileToTargetAction>
{
    public ThrowProjectileToTargetExecutor(ThrowProjectileToTargetAction data) : base(data) { }

    public override void Act(GameObject owner, GameObject target)
    {
        if (Data.Projectile == null)
        {
            Debug.LogError("No projectile prefab on " + Data.name + ".", Data);
            return;
        }

        var projectile = Object.Instantiate(Data.Projectile.gameObject, owner.transform.position, Quaternion.identity);

        projectile.GetComponent<ThrownTargetedProjectile>()
            .StartThrow(Hit, owner, target, Data.ProjectileSpeed);
    }

    /// <summary>
    /// Checked on arrival, not when the order was given: the target had time to
    /// change sides or become invulnerable while the projectile was in the air.
    /// </summary>
    private void Hit(GameObject target, GameObject skillOwner)
    {
        if (!SkillTargetFilter.CanHit(target, skillOwner, Data.TargetType))
        {
            return;
        }

        SkillImpactExecutorFactory.ApplyUnitImpacts(Data, target, skillOwner);
    }
}
