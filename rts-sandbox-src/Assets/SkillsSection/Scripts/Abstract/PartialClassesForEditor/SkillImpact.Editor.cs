using UnityEngine;

public abstract partial class SkillImpact
{
    //Not good solution to understand in editor what imnpact is, maybe will be exchanged on castom drawer to draw the label.
    [SerializeField] private string _impactType;
    public string ImpactType => _impactType;

    public void Initialize()
    {
        _impactType = GetType().Name;
    }
}
