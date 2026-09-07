using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The only place that knows which class applies which impact. Adding an impact
/// is a new data class, a new SkillImpactType value and one line here.
/// </summary>
public static class SkillImpactExecutorFactory
{
    public static SkillImpactExecutor Create(SkillImpact impact)
    {
        switch (impact.Type)
        {
            case SkillImpactType.InstantDamage:
                return new InstantDamageExecutor((InstantDamageImpact)impact);
            case SkillImpactType.PoisonDamage:
                return new PoisonDamageExecutor((PoisonDamageImpact)impact);
            case SkillImpactType.Stun:
                return new StunExecutor((StunImpact)impact);
            default:
                Debug.LogError("No executor for impact " + impact.Type + ".");
                return null;
        }
    }

    /// <summary>Applies every impact of an action that lands on a unit.</summary>
    public static void ApplyUnitImpacts(SkillAction action, GameObject target, GameObject skillOwner)
    {
        ApplyUnitImpacts(action.Impacts, target, skillOwner);
    }

    /// <summary>
    /// The same for a list given explicitly: an action may carry more than one
    /// set of impacts, like the blast and the zone tick of a delayed explosion.
    /// </summary>
    public static void ApplyUnitImpacts(IEnumerable<SkillImpact> impacts, GameObject target, GameObject skillOwner)
    {
        if (impacts == null)
        {
            return;
        }

        foreach (var impact in impacts)
        {
            if (impact is not SkillUnitImpact)
            {
                continue;
            }

            Create(impact)?.ApplyToUnit(target, skillOwner);
        }
    }
}
