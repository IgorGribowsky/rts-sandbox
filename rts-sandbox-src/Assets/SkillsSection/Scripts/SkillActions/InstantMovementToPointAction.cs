
using UnityEngine;

[CreateAssetMenu(fileName = "NewInstantMovementToPointAction", menuName = "Game/SkillActions/Instant Movement To Point Action")]
public class InstantMovementToPointAction : CastToPointAction
{
    public float MaxRange;

    public override void Act()
    {
        InstantlyMove();

        foreach (var impact in Impacts)
        {
            if (impact is SkillUnitImpact unitImpact)
            {
                unitImpact.ImpactToUnit(Skill.SkillOwner);
            }
        }
    }

    private void InstantlyMove()
    {
        var owner = Skill.SkillOwner.transform;

        var target = CastPoint;
        target.y = owner.position.y;

        var delta = target - owner.position;
        if (delta.sqrMagnitude > MaxRange * MaxRange)
        {
            target = owner.position + delta.normalized * MaxRange;
        }

        var navMeshMovement = Skill.SkillOwner.GetComponent<NavMeshMovement>();

        if (navMeshMovement != null)
        {
            navMeshMovement.Warp(target);
        }
        else
        {
            owner.position = target;
        }
    }
}