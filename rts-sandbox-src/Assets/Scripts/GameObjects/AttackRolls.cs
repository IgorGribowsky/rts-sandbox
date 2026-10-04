using RtsSandbox.Rules;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// What may change an ordinary blow of this unit at the moment it goes out —
/// for now critical strikes (T-070). A passive puts its chance here while it
/// works and takes it away when it stops. Holds runtime state only, so nobody
/// places it by hand: the first passive that needs it adds it.
/// </summary>
public class AttackRolls : MonoBehaviour
{
    private readonly Dictionary<object, CritChance> _crits = new Dictionary<object, CritChance>();
    private readonly List<CritChance> _list = new List<CritChance>();

    public static AttackRolls GetOrAdd(GameObject unit)
    {
        if (unit == null)
        {
            return null;
        }

        var rolls = unit.GetComponent<AttackRolls>();
        return rolls != null ? rolls : unit.AddComponent<AttackRolls>();
    }

    /// <summary>
    /// The blow of this attacker with crits rolled. An attacker without any
    /// crit source hits as it is.
    /// </summary>
    public static float Roll(GameObject attacker, float damage, out bool critical)
    {
        var rolls = attacker != null ? attacker.GetComponent<AttackRolls>() : null;
        if (rolls == null)
        {
            critical = false;
            return damage;
        }

        return CriticalStrikeRules.Resolve(damage, rolls._list, () => Random.value, out critical);
    }

    public void AddCrit(object source, float chancePercent, float multiplier)
    {
        _crits[source] = new CritChance(chancePercent, multiplier);
        Rebuild();
    }

    public void RemoveCrit(object source)
    {
        if (_crits.Remove(source))
        {
            Rebuild();
        }
    }

    private void Rebuild()
    {
        _list.Clear();
        _list.AddRange(_crits.Values);
    }
}
