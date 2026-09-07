using UnityEngine;

/// <summary>
/// Applies one impact of a skill.
/// </summary>
public abstract class SkillImpactExecutor
{
    public abstract void ApplyToUnit(GameObject target, GameObject skillOwner);
}

public abstract class SkillImpactExecutor<TImpact> : SkillImpactExecutor
    where TImpact : SkillImpact
{
    protected readonly TImpact Data;

    protected SkillImpactExecutor(TImpact data)
    {
        Data = data;
    }
}
