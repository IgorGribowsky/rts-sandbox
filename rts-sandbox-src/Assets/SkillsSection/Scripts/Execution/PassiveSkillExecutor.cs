using UnityEngine;

/// <summary>
/// Runs one passive skill. Unlike an active one it is NOT built at the moment of
/// a cast and thrown away: a passive works while the unit is alive, and the
/// executor is what holds its subscription, so it lives exactly as long as the
/// unit does (M-015).
/// </summary>
public abstract class PassiveSkillExecutor
{
    /// <summary>The unit appeared: the passive starts working.</summary>
    public abstract void Activate(GameObject owner);

    /// <summary>
    /// The passive stops working: the unit died or the skill was taken away.
    /// Whatever Activate subscribed to has to be released here.
    /// </summary>
    public abstract void Deactivate(GameObject owner);
}

/// <summary>
/// Executor of a passive described by a specific data asset.
/// </summary>
public abstract class PassiveSkillExecutor<TAction> : PassiveSkillExecutor
    where TAction : PassiveSkillAction
{
    protected readonly TAction Data;

    protected PassiveSkillExecutor(TAction data)
    {
        Data = data;
    }
}
