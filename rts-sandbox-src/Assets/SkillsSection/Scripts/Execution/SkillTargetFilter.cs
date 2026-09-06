using Assets.Scripts;
using UnityEngine;

/// <summary>
/// Who a skill is allowed to hit. Was CanHitCheck inside ThrowProjectileAction,
/// with the author's note that it belongs in a shared helper.
/// </summary>
public static class SkillTargetFilter
{
    public static bool CanHit(GameObject target, GameObject skillOwner, TargetType targetType)
    {
        if (target == skillOwner)
        {
            return false;
        }

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

        switch (targetType)
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
