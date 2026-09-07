using UnityEngine;

/// <summary>
/// The unit does nothing while this hangs on it (M-019). The effect itself does
/// not stop anything: it raises StunStarted, and the stunned behaviour — which
/// smothers every other behaviour the way any behaviour does — is switched on by
/// the behaviour manager. Whatever takes the effect off, the behaviour goes with
/// it: the duration ran out, someone removed it, the unit died.
///
/// Structure chosen by the user (answer in chat 2026-09-05).
/// </summary>
public class StunEffect : UnitEffect
{
    private UnitEventManager _targetEvents;

    public StunEffect(object key, GameObject source, float duration)
        : base(key, source, duration)
    {
    }

    internal override void OnApplied(GameObject target)
    {
        _targetEvents = target.GetComponent<UnitEventManager>();
        _targetEvents?.OnStunStarted();
    }

    internal override void OnRemoved(GameObject target)
    {
        // Two stuns from two different skills hang in parallel, each under its own
        // key (M-019). The first of them to run out must not wake the unit up while
        // the other is still on it. By this point the holder has already dropped
        // this effect, so what it still has is only the others.
        var effects = target == null ? null : target.GetComponent<UnitEffects>();

        if (effects != null && effects.HasAny<StunEffect>())
        {
            return;
        }

        _targetEvents?.OnStunEnded();
    }
}
