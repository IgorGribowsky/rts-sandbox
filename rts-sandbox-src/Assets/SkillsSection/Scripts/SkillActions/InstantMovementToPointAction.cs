using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "NewInstantMovementToPointAction", menuName = "Game/SkillActions/Instant Movement To Point Action")]
public class InstantMovementToPointAction : CastToPointAction
{
    /// <summary>
    /// Renamed behind FormerlySerializedAs, not retyped: the numbers already in
    /// the assets must survive, and the name is taken by the property below.
    /// </summary>
    [FormerlySerializedAs("MaxRange")]
    [SerializeField]
    private float _maxRange;

    /// <summary>The jump never goes further, whatever the CastRange of the skill is.</summary>
    public override float MaxRange => _maxRange;

    public override SkillActionType Type => SkillActionType.InstantMovementToPoint;
}
