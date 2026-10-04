using System.Collections.Generic;

/// <summary>
/// Which skill an action or an impact belongs to (T-073). An effect only knows
/// its Key — the impact data that put it on — and the HUD asks here for the
/// skill behind it to borrow its picture and description.
///
/// Filled by UnitSkills as units wake up. An asset of an action belongs to one
/// skill, so one entry per key is enough.
/// </summary>
public static class SkillCatalog
{
    private static readonly Dictionary<object, Skill> BySource = new Dictionary<object, Skill>();

    public static void Register(Skill skill)
    {
        if (skill == null)
        {
            return;
        }

        SkillAction action = skill is ActiveSkill active ? active.Action
            : skill is PassiveSkill passive ? passive.Action
            : null;

        if (action == null)
        {
            return;
        }

        BySource[action] = skill;
        RegisterImpacts(action.Impacts, skill);

        if (action is DelayedExplosionAction explosion)
        {
            RegisterImpacts(explosion.ZoneImpacts, skill);
        }
    }

    /// <summary>The skill that carries this action or impact, null when none does.</summary>
    public static Skill SkillOf(object source)
    {
        if (source == null)
        {
            return null;
        }

        return BySource.TryGetValue(source, out var skill) ? skill : null;
    }

    private static void RegisterImpacts(IEnumerable<SkillImpact> impacts, Skill skill)
    {
        if (impacts == null)
        {
            return;
        }

        foreach (var impact in impacts)
        {
            if (impact != null)
            {
                BySource[impact] = skill;
            }
        }
    }
}
