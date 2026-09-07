using UnityEngine;

/// <summary>
/// A passive of the "goes off on a circumstance" kind (M-015): the circumstance
/// is the owner landing an ordinary attack, and what happens is this action's
/// impacts applied to whoever was hit.
///
/// The kind is deliberately not a "pick an event from a dropdown" thing: M-015
/// says the circumstance is decided per passive, so a new circumstance is a new
/// data class and a new executor, not a new enum value in the inspector.
///
/// "Poisonous attack" is an asset of this kind with a poison impact inside.
/// </summary>
[CreateAssetMenu(fileName = "NewApplyImpactsOnDamageDealtAction", menuName = "Game/SkillActions/Apply Impacts On Damage Dealt Action")]
public class ApplyImpactsOnDamageDealtAction : PassiveSkillAction, ITargetSelected
{
    [SerializeField]
    public TargetType _targetType = TargetType.Enemies;
    public TargetType TargetType => _targetType;

    public override SkillActionType Type => SkillActionType.ApplyImpactsOnDamageDealt;
}
