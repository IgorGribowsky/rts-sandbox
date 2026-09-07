using UnityEngine;

/// <summary>
/// The only place that knows which class runs which passive. Adding a kind of
/// passive is a new data class, a new SkillActionType value and one line here.
/// An explicit switch and not reflection on purpose: managed code stripping
/// drops classes nobody references by name.
/// </summary>
public static class PassiveSkillExecutorFactory
{
    public static PassiveSkillExecutor Create(PassiveSkillAction action)
    {
        if (action == null)
        {
            return null;
        }

        switch (action.Type)
        {
            default:
                Debug.LogError("No executor for passive skill action " + action.Type + ".");
                return null;
        }
    }
}
