using UnityEngine;

[CreateAssetMenu(fileName = "NewInstantMovementToPointAction", menuName = "Game/SkillActions/Instant Movement To Point Action")]
public class InstantMovementToPointAction : CastToPointAction
{
    public float MaxRange;

    public override SkillActionType Type => SkillActionType.InstantMovementToPoint;
}
