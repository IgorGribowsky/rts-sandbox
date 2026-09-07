using UnityEngine;

/// <summary>
/// Puts the zone object down at the cast point and hands it its numbers. The
/// delay, the blast and the ticks are the zone's own business from there, so the
/// caster is free — and free to die without cancelling the blast.
/// </summary>
public class DelayedExplosionExecutor : CastToPointActionExecutor<DelayedExplosionAction>
{
    public DelayedExplosionExecutor(DelayedExplosionAction data) : base(data) { }

    public override void Act(GameObject owner, Vector3 castPoint)
    {
        if (Data.Zone == null)
        {
            Debug.LogError("No zone prefab on " + Data.name + ".", Data);
            return;
        }

        var teamMember = owner.GetComponent<TeamMember>();

        if (teamMember == null)
        {
            return;
        }

        var zone = Object.Instantiate(Data.Zone.gameObject, castPoint, Quaternion.identity);

        zone.GetComponent<DelayedExplosionZone>().StartZone(
            owner: owner,
            ownerTeamId: teamMember.TeamId,
            targetType: Data.TargetType,
            radius: Data.AreaRadius,
            delay: Data.Delay,
            zoneDuration: Data.ZoneDuration,
            tickRate: Data.ZoneDamageRate,
            blast: Blast,
            tick: Tick);
    }

    private void Blast(GameObject target, GameObject skillOwner)
    {
        SkillImpactExecutorFactory.ApplyUnitImpacts(Data.Impacts, target, skillOwner);
    }

    private void Tick(GameObject target, GameObject skillOwner)
    {
        SkillImpactExecutorFactory.ApplyUnitImpacts(Data.ZoneImpacts, target, skillOwner);
    }
}
