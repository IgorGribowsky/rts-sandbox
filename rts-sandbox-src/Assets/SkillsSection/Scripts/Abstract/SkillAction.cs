using System.Collections.Generic;
using UnityEngine;

public abstract class SkillAction : ScriptableObject
{
    public TargetType ImpactType;

    public List<SkillImpact> Impacts;

    //Navigation property
    public Skill Skill { get; set; }
}
