
using Assets.Scripts;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

[CreateAssetMenu(fileName = "NewThrowProjectileAction", menuName = "Game/SkillActions/Throw Projectile Action")]
public class ThrowProjectileAction : CastToPointAction, ITargetSelected
{
    public float ProjectileSpeed;

    public float ProjectileRange;

    public ThrownProjectile Projectile;

    [SerializeField]
    public TargetType _targetType = TargetType.Enemies;
    public TargetType TargetType => _targetType;

    public override void Act(GameObject owner, Vector3 castPoint)
    {
        var projectile = Instantiate(Projectile.gameObject, owner.transform.position, Quaternion.identity);
        var projectileScript = projectile.GetComponent<ThrownProjectile>();
        var direction = castPoint - owner.transform.position;
        direction.y = 0;

        projectileScript.StartThrow(CanHitCheck, HitProjectile, owner, direction, ProjectileRange, ProjectileSpeed);
    }

    public void HitProjectile(GameObject target, GameObject skillOwner)
    {
        foreach (var impact in Impacts)
        {
            if (impact is SkillUnitImpact unitImpact)
            {
                unitImpact.ImpactToUnit(target, skillOwner);
            }
        }
    }

    //part of code can be moved to common helper class when another actions will be implemented and common code finded
    public bool CanHitCheck(GameObject target, GameObject skillOwner)
    {
        var skillOwnerTeam = skillOwner.GetComponent<TeamMember>().TeamId;
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

        switch (TargetType)
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