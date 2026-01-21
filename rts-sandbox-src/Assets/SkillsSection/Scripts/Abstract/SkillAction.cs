using System.Collections.Generic;
using UnityEngine;

public abstract partial class SkillAction : ScriptableObject
{
    public TargetType TargetType;

    [SerializeReference]
    public List<SkillImpact> Impacts;

    //Navigation property
    public Skill Skill { get; set; }
}
