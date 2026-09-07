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
        return CanHit(target, skillOwner.GetComponent<TeamMember>().TeamId, skillOwner, targetType);
    }

    /// <summary>
    /// The same check for something that outlives its caster — a zone on the
    /// ground, for instance. The team is passed as a number because the owner's
    /// TeamMember may already be gone; skillOwner is then only used to keep the
    /// caster out of his own area, and may be null.
    /// </summary>
    public static bool CanHit(GameObject target, int skillOwnerTeam, GameObject skillOwner, TargetType targetType)
    {
        if (skillOwner != null && target == skillOwner)
        {
            return false;
        }

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
