
using UnityEngine;

[CreateAssetMenu(fileName = "NewThrowWaveProjectileAction", menuName = "Game/SkillActions/Throw Wave Projectile Action")]
public class ThrowWaveProjectileAction : ThrowProjectileAction
{
    public float WaveWidthScale = 1f;

    public override void Act(GameObject owner, Vector3 castPoint)
    {
        var projectile = Instantiate(Projectile, owner.transform.position, Quaternion.identity);
        var projectileScript = projectile.GetComponent<ThrownWaveProjectile>();
        var direction = castPoint - owner.transform.position;
        direction.y = 0;

        projectileScript.StartThrow(CanHitCheck, HitProjectile, owner, direction, ProjectileRange, ProjectileSpeed, WaveWidthScale);
    }
}