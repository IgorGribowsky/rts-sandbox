using System;

[Serializable]
public abstract partial class SkillImpact
{
    //Navigation property
    public SkillAction SkillAction { get; set; }
}
