using UnityEngine;

/// <summary>
/// How an effect looks in the HUD (T-073): its name, picture and description.
/// Optional — an impact without one shows the picture, name and description
/// of the skill that put the effect on, which is enough for most of them. The
/// stun has one of its own: a stun from any skill reads as a stun.
/// </summary>
[CreateAssetMenu(fileName = "NewEffectInfo", menuName = "Game/Effects/Effect Info")]
public class EffectInfo : ScriptableObject
{
    public string Name;

    [Tooltip("The picture on the effect badge. Empty: the picture of the skill that put the effect on.")]
    public Texture2D Icon;

    [Tooltip("What the effect does to the unit, for the tooltip. Empty: the description of the skill.")]
    [TextArea(2, 5)]
    public string Description;
}
