
using UnityEngine;

[CreateAssetMenu(fileName = "NewInstantMovementToPointAction", menuName = "Game/SkillActions/Instant Movement To Point Action")]
public class InstantMovementToPointAction : CastToPointAction
{
    public float MaxRange;

    public override void Act(GameObject owner, Vector3 castPoint)
    {
        InstantlyMove(owner, castPoint);

        foreach (var impact in Impacts)
        {
            if (impact is SkillUnitImpact unitImpact)
            {
                unitImpact.ImpactToUnit(owner, owner);
            }
        }
    }

    private void InstantlyMove(GameObject owner, Vector3 castPoint)
    {
        var ownerTransform = owner.transform;
        var target = castPoint;
        target.y = ownerTransform.position.y;

        var delta = target - ownerTransform.position;
        if (delta.sqrMagnitude > MaxRange * MaxRange)
        {
            target = ownerTransform.position + delta.normalized * MaxRange;
        }

        var navMeshMovement = owner.GetComponent<NavMeshMovement>();

        if (navMeshMovement != null)
        {
            navMeshMovement.Warp(target);
        }
        else
        {
            ownerTransform.position = target;
        }
    }
}