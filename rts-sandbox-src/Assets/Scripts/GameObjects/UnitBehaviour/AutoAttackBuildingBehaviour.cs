using Assets.Scripts.Infrastructure.Constants;
using Assets.Scripts.Infrastructure.Events;
using UnityEngine;

public class AutoAttackBuildingBehaviour : AutoAttackingBehaviourBase
{
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
