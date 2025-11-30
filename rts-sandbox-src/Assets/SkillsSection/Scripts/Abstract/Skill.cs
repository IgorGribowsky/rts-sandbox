using UnityEngine;

public abstract class Skill : ScriptableObject
{
    public string Name;
    public float Cooldown;

    //Navigation property
    public GameObject SkillOwner { get; set; }
}
