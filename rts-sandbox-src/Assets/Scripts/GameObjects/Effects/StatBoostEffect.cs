using UnityEngine;

/// <summary>
/// For a while the unit hits harder and faster (M-019, T-053). Percent on top
/// of the unit's own numbers; two boosts from two skills add up, the same skill
/// again only starts the countdown over, like any effect.
/// </summary>
public class StatBoostEffect : UnitEffect
{
    private readonly float _damagePercent;
    private readonly float _attackSpeedPercent;

    public StatBoostEffect(object key, GameObject source, float duration, float damagePercent, float attackSpeedPercent)
        : base(key, source, duration)
    {
        _damagePercent = damagePercent;
        _attackSpeedPercent = attackSpeedPercent;
    }

    public float DamagePercent => _damagePercent;
    public float AttackSpeedPercent => _attackSpeedPercent;

    internal override void OnApplied(GameObject target)
    {
        var values = target.GetComponent<UnitValues>();
        if (values != null)
        {
            values.AddBoost(_damagePercent, _attackSpeedPercent);
        }
    }

    internal override void OnRemoved(GameObject target)
    {
        var values = target == null ? null : target.GetComponent<UnitValues>();
        if (values != null)
        {
            values.AddBoost(-_damagePercent, -_attackSpeedPercent);
        }
    }
}
