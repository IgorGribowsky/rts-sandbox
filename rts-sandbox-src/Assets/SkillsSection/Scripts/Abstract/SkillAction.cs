using System.Collections.Generic;
using UnityEngine;

public abstract partial class SkillAction : ScriptableObject
{
    [SerializeReference]
    public List<SkillImpact> Impacts;

    //Navigation property
    public Skill Skill { get; set; }
}
