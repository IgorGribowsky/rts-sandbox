using UnityEngine;

/// <summary>
/// Passive critical strike (T-070): with a chance an ordinary blow — melee or
/// a projectile — is multiplied, and red numbers of the damage float up over
/// the attacker. The chance is rolled when the blow goes out.
/// </summary>
[CreateAssetMenu(fileName = "NewCriticalStrikeAction", menuName = "Game/SkillActions/Critical Strike Action")]
public class CriticalStrikeAction : PassiveSkillAction
{
    [Tooltip("Chance of a crit on an ordinary blow, percent.")]
    [Range(0f, 100f)]
    public float ChancePercent = 15f;

    [Tooltip("The blow is multiplied by this on a crit.")]
    public float Multiplier = 2f;

    public override SkillActionType Type => SkillActionType.CriticalStrike;
}
