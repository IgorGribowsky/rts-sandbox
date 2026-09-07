using Assets.Scripts.Infrastructure.Constants;
using UnityEngine;

/// <summary>
/// Same idea as the context menu on SkillAction, but for the second list of
/// impacts: the one the zone tick uses.
/// </summary>
public partial class DelayedExplosionAction
{
    [ContextMenu("Add Instant Damage Impact To Zone")]
    void AddInstantDamageToZone()
    {
        ZoneImpacts ??= new();
        var impact = new InstantDamageImpact { damage = default, type = default };
        impact.Initialize();
        ZoneImpacts.Add(impact);
    }

    [ContextMenu("Add Poison Damage Impact To Zone")]
    void AddPoisonDamageToZone()
    {
        ZoneImpacts ??= new();
        var impact = new PoisonDamageImpact
        {
            dps = default,
            type = default,
            duration = default,
            tickInterval = GameConstants.DefaultEffectTickRate,
        };
        impact.Initialize();
        ZoneImpacts.Add(impact);
    }
}
