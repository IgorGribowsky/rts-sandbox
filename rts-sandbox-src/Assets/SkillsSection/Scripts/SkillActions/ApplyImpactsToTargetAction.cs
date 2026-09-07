using UnityEngine;

/// <summary>
/// The simplest action with a unit for a target: whatever impacts it carries land
/// on that unit the moment the cast finishes. No projectile — a projectile flying
/// at a target is a kind of its own and belongs to the skill that needs it.
/// </summary>
[CreateAssetMenu(fileName = "NewApplyImpactsToTargetAction", menuName = "Game/SkillActions/Apply Impacts To Target Action")]
public class ApplyImpactsToTargetAction : CastToTargetAction
{
    [SerializeField]
    public TargetType _targetType = TargetType.Enemies;
    public override TargetType TargetType => _targetType;

    public override SkillActionType Type => SkillActionType.ApplyImpactsToTarget;
}
