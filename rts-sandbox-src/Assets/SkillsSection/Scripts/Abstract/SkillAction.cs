using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Data of one skill action: numbers, prefab references and the impacts it
/// carries. No execution here on purpose — that lives in SkillActionExecutor.
/// </summary>
public abstract partial class SkillAction : ScriptableObject
{
    [SerializeReference]
    public List<SkillImpact> Impacts;

    public abstract SkillActionType Type { get; }
}
