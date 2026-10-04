public class ApplyImpactsToSelfExecutor : SkillActionExecutor<ApplyImpactsToSelfAction>
{
    public ApplyImpactsToSelfExecutor(ApplyImpactsToSelfAction data) : base(data) { }

    public override void Act(SkillParams skillParams)
    {
        if (skillParams.Owner == null)
        {
            return;
        }

        SkillImpactExecutorFactory.ApplyUnitImpacts(Data, skillParams.Owner, skillParams.Owner);
    }
}
