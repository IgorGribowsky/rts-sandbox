using System;

/// <summary>
/// Data of one impact, stored inside a skill action asset. Applying it is
/// the job of SkillImpactExecutor.
/// </summary>
[Serializable]
public abstract partial class SkillImpact
{
    public abstract SkillImpactType Type { get; }
}
