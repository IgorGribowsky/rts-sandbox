using UnityEngine;

public class InstantMovementToPointExecutor : CastToPointActionExecutor<InstantMovementToPointAction>
{
    public InstantMovementToPointExecutor(InstantMovementToPointAction data) : base(data) { }

    public override void Act(GameObject owner, Vector3 castPoint)
    {
        InstantlyMove(owner, castPoint);

        SkillImpactExecutorFactory.ApplyUnitImpacts(Data, owner, owner);
    }

    private void InstantlyMove(GameObject owner, Vector3 castPoint)
    {
        var ownerTransform = owner.transform;
        var target = castPoint;
        target.y = ownerTransform.position.y;

        var delta = target - ownerTransform.position;
        if (delta.sqrMagnitude > Data.MaxRange * Data.MaxRange)
        {
            target = ownerTransform.position + delta.normalized * Data.MaxRange;
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
