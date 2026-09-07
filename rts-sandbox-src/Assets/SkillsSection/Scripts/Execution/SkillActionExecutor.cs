/// <summary>
/// Runs one skill action. Holds no state of its own beyond the data it reads,
/// so it is built at the moment of the cast and thrown away after.
/// </summary>
public abstract class SkillActionExecutor
{
    public abstract void Act(SkillParams skillParams);
}

/// <summary>
/// Executor of an action described by a specific data asset.
/// </summary>
public abstract class SkillActionExecutor<TAction> : SkillActionExecutor
    where TAction : SkillAction
{
    protected readonly TAction Data;

    protected SkillActionExecutor(TAction data)
    {
        Data = data;
    }
}
