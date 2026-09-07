using UnityEngine;

/// <summary>
/// The only place that knows which class runs which action. Adding a kind of
/// skill is a new data class, a new SkillActionType value and one line here.
/// An explicit switch and not reflection on purpose: managed code stripping
/// drops classes nobody references by name.
/// </summary>
public static class SkillActionExecutorFactory
{
    public static SkillActionExecutor Create(SkillAction action)
    {
        switch (action.Type)
        {
            case SkillActionType.ThrowProjectile:
                return new ThrowProjectileExecutor((ThrowProjectileAction)action);
            case SkillActionType.ThrowWaveProjectile:
                return new ThrowWaveProjectileExecutor((ThrowWaveProjectileAction)action);
            case SkillActionType.InstantMovementToPoint:
                return new InstantMovementToPointExecutor((InstantMovementToPointAction)action);
            case SkillActionType.ApplyImpactsToTarget:
                return new ApplyImpactsToTargetExecutor((ApplyImpactsToTargetAction)action);
            case SkillActionType.ThrowProjectileToTarget:
                return new ThrowProjectileToTargetExecutor((ThrowProjectileToTargetAction)action);
            case SkillActionType.DelayedExplosion:
                return new DelayedExplosionExecutor((DelayedExplosionAction)action);
            default:
                Debug.LogError("No executor for skill action " + action.Type + ".");
                return null;
        }
    }
}
