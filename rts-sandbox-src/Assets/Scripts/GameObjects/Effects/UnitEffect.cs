using UnityEngine;

/// <summary>
/// One effect hanging on a unit while the game runs: a duration, a tick of its
/// own and removal when the duration is over. Never serialized — it is built at
/// the moment the effect lands and dies with the unit, so nothing of it is ever
/// written into a shared asset (M-019).
/// </summary>
public abstract class UnitEffect
{
    protected UnitEffect(object key, GameObject source, float duration)
    {
        Key = key;
        Source = source;
        Duration = duration;
        Remaining = duration;
    }

    /// <summary>
    /// What makes two effects "the same one". It is the data of whatever applied
    /// the effect: one action asset belongs to one skill, so two casters of the
    /// same skill share a single effect and refresh it, while the same kind of
    /// effect from another skill hangs in parallel (M-019).
    /// </summary>
    public object Key { get; }

    /// <summary>
    /// Who applied it. May die before the effect runs out — then this is null and
    /// the effect keeps working without an attacker.
    /// </summary>
    public GameObject Source { get; private set; }

    public float Duration { get; }

    public float Remaining { get; private set; }

    public bool IsExpired => Remaining <= 0;

    /// <summary>
    /// Applied again: nothing stacks, the countdown just starts over (M-019).
    /// The new source replaces the old one — it is the one keeping the effect up.
    /// </summary>
    internal virtual void Refresh(GameObject source)
    {
        Source = source;
        Remaining = Duration;
    }

    /// <summary>
    /// Advances the effect. The step is clipped to what is left of the duration,
    /// so an effect never does anything past its own lifetime.
    /// </summary>
    internal void Tick(GameObject target, float deltaTime)
    {
        if (Remaining <= 0)
        {
            return;
        }

        var step = Mathf.Min(deltaTime, Remaining);
        Remaining -= step;

        OnTick(target, step);
    }

    internal virtual void OnApplied(GameObject target) { }

    internal virtual void OnTick(GameObject target, float deltaTime) { }

    /// <summary>Called for any reason the effect goes away: expired, removed, unit died.</summary>
    internal virtual void OnRemoved(GameObject target) { }
}
