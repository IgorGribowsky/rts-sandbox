using UnityEngine;

public abstract class Skill : ScriptableObject
{
    public string Name;
    public float Cooldown;

    [Tooltip("The picture on the skill button in the HUD (M-023).")]
    public Texture2D Icon;

    [Tooltip("What the skill does, in a sentence or two, for the tooltip (M-023).")]
    [TextArea(2, 5)]
    public string Description;
}
