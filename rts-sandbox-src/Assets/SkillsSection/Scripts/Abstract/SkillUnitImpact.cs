using System;
using UnityEngine;

[Serializable]
public abstract class SkillUnitImpact : SkillImpact
{
    public abstract void ImpactToUnit(GameObject target);
}
