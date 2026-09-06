using Assets.Scripts.GameObjects.UnitBehaviour;

public class AutoAttackBuildingBehaviour : AutoAttackingBehaviourBase
{
    public override UnitActionType Trigger => UnitActionType.AutoAttackIdle;

    protected override void IfNoTargetUpdate()
    {
    }

    protected override void IfTargetExistsUpdate()
    {
    }

    protected override void UpdateAction()
    {
        FindNearestTargetAndAct();

        if (!_triggeredOnEnemy)
        {
            IfNoTargetUpdate();
        }
        else
        {
            IfTargetExistsUpdate();
        }
    }
}
