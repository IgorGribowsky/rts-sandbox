
using UnityEngine;

[CreateAssetMenu(fileName = "NewThrowWaveProjectileAction", menuName = "Game/SkillActions/Throw Wave Projectile Action")]
public class ThrowWaveProjectileAction : ThrowProjectileAction
{
    public float WaveWidthScale = 1f;

    public override void Act()
    {
        var projectile = Instantiate(Projectile, Skill.SkillOwner.transform.position, Quaternion.identity);
        var projectileScript = projectile.GetComponent<ThrownWaveProjectile>();
        var direction = CastPoint - Skill.SkillOwner.transform.position;
        direction.y = 0;

        projectileScript.StartThrow(CanHitCheck, HitProjectile, direction, ProjectileRange, ProjectileSpeed, WaveWidthScale);
    }
}