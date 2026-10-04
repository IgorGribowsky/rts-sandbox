using UnityEngine;

/// <summary>
/// The impacts land on the caster itself, at once (T-053): a boost, a heal of
/// one's own. The first such skill is the Giant's Rage.
/// </summary>
[CreateAssetMenu(fileName = "NewApplyImpactsToSelfAction", menuName = "Game/SkillActions/Apply Impacts To Self Action")]
public class ApplyImpactsToSelfAction : CastWithoutTargetAction
{
    public override SkillActionType Type => SkillActionType.ApplyImpactsToSelf;
}
