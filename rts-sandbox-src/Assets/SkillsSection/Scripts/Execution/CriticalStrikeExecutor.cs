using UnityEngine;

/// <summary>
/// Puts the crit chance on the owner's AttackRolls while the owner lives. The
/// roll itself happens in the attack, where the blow is made (T-070).
/// </summary>
public class CriticalStrikeExecutor : PassiveSkillExecutor<CriticalStrikeAction>
{
    public CriticalStrikeExecutor(CriticalStrikeAction data) : base(data) { }

    public override void Activate(GameObject owner)
    {
        AttackRolls.GetOrAdd(owner)?.AddCrit(Data, Data.ChancePercent, Data.Multiplier);
    }

    public override void Deactivate(GameObject owner)
    {
        var rolls = owner != null ? owner.GetComponent<AttackRolls>() : null;
        if (rolls != null)
        {
            rolls.RemoveCrit(Data);
        }
    }
}
