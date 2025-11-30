
using Assets.Scripts;
using Assets.Scripts.Infrastructure.Enums;
using UnityEngine;

[CreateAssetMenu(fileName = "NewThrowProjectileAction", menuName = "Game/SkillActions/Throw Projectile Action")]
public class ThrowProjectileAction : CastToPointAction
{
    //Temp. Move to impacts
    public float Damage;

    public float ProjectileSpeed;

    public float ProjectileRadius;

    public GameObject Projectile;

    public override void Act()
    {
        var projectile = Instantiate(Projectile, Skill.SkillOwner.transform.position, Quaternion.identity);
        var projectileScript = projectile.GetComponent<ThrownSkillProjectile>();
        var direction = CastPoint - Skill.SkillOwner.transform.position;
        direction.y = 0;

        projectileScript.StartThrow(CanHitCheck, HitProjectile, direction, ProjectileRadius, ProjectileSpeed);
    }

    public void HitProjectile(GameObject target)
    {
        target.GetComponent<UnitEventManager>().OnDamageReceived(
            attacker: Skill.SkillOwner,
            damageAmount: Damage,
            damageType: DamageType.Magic);
    }

    //Move to base class with predicate param addition rules 
    public bool CanHitCheck(GameObject target)
    {
        var skillOwnerTeam = Skill.SkillOwner.GetComponent<TeamMember>().TeamId;
        var targetTeamScript = target.GetComponent<TeamMember>();
        if (targetTeamScript == null)
        {
            return false;
        }

        var targetUnitValues = target.GetComponent<UnitValues>();
        if (targetUnitValues == null || targetUnitValues.IsBuilding || targetUnitValues.IsInvulnerable)
        {
            return false;
        }

        var targetTeam = targetTeamScript.TeamId;

        switch (ImpactType)
        {
            case TargetType.Allies:
                return GameServices.TeamController.GetAllyTeams(skillOwnerTeam).Contains(targetTeam);
            case TargetType.All:
                return true;
            case TargetType.Enemies:
                return GameServices.TeamController.GetEnemyTeams(skillOwnerTeam).Contains(targetTeam);
            default:
                return false;
        }
        
    }
}