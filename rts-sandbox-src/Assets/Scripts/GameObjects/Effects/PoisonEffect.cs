using Assets.Scripts.Infrastructure.Enums;
using UnityEngine;

/// <summary>
/// Periodic damage: dps * tickInterval lands every tickInterval seconds while the
/// effect lasts. The damage goes out as the ordinary OnDamageReceived, so it runs
/// through combat (M-007), death and the call for help (M-008) exactly like a hit
/// from a weapon (M-019).
///
/// Knows nothing about skills on purpose: a skill impact, a passive or a plain
/// attack all put the same effect on a unit, they only differ in the key.
/// </summary>
public class PoisonEffect : UnitEffect
{
    private readonly float _dps;

    private readonly DamageType _damageType;

    private readonly float _tickInterval;

    private const float MinTickInterval = 0.01f;

    private float _untilNextTick;

    private UnitEventManager _targetEvents;

    private UnitValues _targetValues;

    public PoisonEffect(object key, GameObject source, float dps, DamageType damageType, float duration, float tickInterval)
        : base(key, source, duration)
    {
        _dps = dps;
        _damageType = damageType;

        // Clamped away from zero: the tick loop below would hang the editor on it.
        _tickInterval = Mathf.Max(tickInterval, MinTickInterval);

        // No damage at the moment it lands: otherwise reapplying the poison would
        // be free instant damage on top of whatever applied it.
        _untilNextTick = _tickInterval;
    }

    internal override void OnApplied(GameObject target)
    {
        _targetEvents = target.GetComponent<UnitEventManager>();
        _targetValues = target.GetComponent<UnitValues>();
    }

    /// <summary>
    /// The rhythm of the ticks is deliberately NOT reset when the poison is
    /// applied again: resetting it would let frequent reapplying push the next
    /// tick away forever and the poison would never do anything.
    /// </summary>
    internal override void OnTick(GameObject target, float deltaTime)
    {
        _untilNextTick -= deltaTime;

        // A long frame can cover more than one interval, so no damage is lost.
        while (_untilNextTick <= 0)
        {
            _untilNextTick += _tickInterval;

            DealTickDamage();
        }
    }

    private void DealTickDamage()
    {
        // Already dead this frame: the object is destroyed at the end of it, but
        // Update can still run, and a corpse must not take another tick.
        if (_targetEvents == null || (_targetValues != null && _targetValues.CurrentHp <= 0))
        {
            return;
        }

        _targetEvents.OnDamageReceived(
            attacker: Source,
            damageAmount: _dps * _tickInterval,
            damageType: _damageType);
    }
}
