using UnityEngine;

public abstract partial class SkillAction : ScriptableObject
{
    [ContextMenu("Add Instant Damage Impact")]
    void AddInstantDamage()
    {
        Impacts ??= new();
        var impact = new InstantDamageImpact { damage = default, type = default };
        impact.Initialize();
        Impacts.Add(impact);
    }

    [ContextMenu("Add Poison Damage Impact")]
    void AddPoisonDamage()
    {
        Impacts ??= new();
        var impact = new PoisonDamageImpact { dps = default, type = default, duration = default };
        impact.Initialize();
        Impacts.Add(impact);
    }
}
