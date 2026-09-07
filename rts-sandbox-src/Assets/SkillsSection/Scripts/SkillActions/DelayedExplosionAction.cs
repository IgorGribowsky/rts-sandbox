using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Cast at an area: after the cast finishes, Delay seconds later a blast hits
/// everyone in the circle, and then a zone stays on the ground for ZoneDuration
/// and hits whoever stands in it every ZoneDamageRate seconds.
///
/// Two lists of impacts on purpose: the blast and the zone tick are different
/// numbers, and either of them can carry anything an impact can — poison
/// included.
/// </summary>
[CreateAssetMenu(fileName = "NewDelayedExplosionAction", menuName = "Game/SkillActions/Delayed Explosion Action")]
public partial class DelayedExplosionAction : CastToAreaAction, ITargetSelected
{
    [Tooltip("Seconds between the end of the cast and the blast. 0 means at once.")]
    public float Delay;

    [Tooltip("How long the zone stays after the blast. 0 means no zone at all.")]
    public float ZoneDuration;

    [Tooltip("How often the zone hits those inside it.")]
    public float ZoneDamageRate;

    /// <summary>What the zone tick does. The blast itself uses Impacts.</summary>
    [SerializeReference]
    public List<SkillImpact> ZoneImpacts;

    public DelayedExplosionZone Zone;

    [SerializeField]
    public TargetType _targetType = TargetType.Enemies;
    public TargetType TargetType => _targetType;

    public override SkillActionType Type => SkillActionType.DelayedExplosion;
}
