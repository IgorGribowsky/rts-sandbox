using Assets.Scripts.Infrastructure.Events;
using UnityEngine;

/// <summary>
/// Holds a subscription to the owner's DamageDealt for as long as the owner
/// lives, and puts the action's impacts on whoever the owner hits.
/// </summary>
public class ApplyImpactsOnDamageDealtExecutor : PassiveSkillExecutor<ApplyImpactsOnDamageDealtAction>
{
    private GameObject _owner;

    private UnitEventManager _ownerEvents;

    public ApplyImpactsOnDamageDealtExecutor(ApplyImpactsOnDamageDealtAction data) : base(data) { }

    public override void Activate(GameObject owner)
    {
        _owner = owner;
        _ownerEvents = owner.GetComponent<UnitEventManager>();

        if (_ownerEvents == null)
        {
            Debug.LogError("A passive needs UnitEventManager on " + owner.name + ".");
            return;
        }

        _ownerEvents.DamageDealt += DamageDealtHandler;
    }

    public override void Deactivate(GameObject owner)
    {
        if (_ownerEvents != null)
        {
            _ownerEvents.DamageDealt -= DamageDealtHandler;
            _ownerEvents = null;
        }

        _owner = null;
    }

    private void DamageDealtHandler(DamageDealtEventArgs args)
    {
        // The same filter the active skills use, so a passive cannot land on
        // buildings, on the invulnerable or on the wrong side.
        if (args.Victim == null || !SkillTargetFilter.CanHit(args.Victim, _owner, Data.TargetType))
        {
            return;
        }

        SkillImpactExecutorFactory.ApplyUnitImpacts(Data, args.Victim, _owner);
    }
}
