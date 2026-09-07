using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The effects hanging on this unit right now. Holds runtime state only and has
/// nothing to configure, so it is not placed on prefabs by hand: whatever lands
/// the first effect adds the component itself through GetOrAdd.
/// </summary>
public class UnitEffects : MonoBehaviour
{
    private readonly List<UnitEffect> _effects = new List<UnitEffect>();

    /// <summary>The holder of this unit, created on the spot if it has none yet.</summary>
    public static UnitEffects GetOrAdd(GameObject unit)
    {
        if (unit == null)
        {
            return null;
        }

        var effects = unit.GetComponent<UnitEffects>();

        return effects != null ? effects : unit.AddComponent<UnitEffects>();
    }

    /// <summary>
    /// Puts an effect on the unit. Something with the same key already hangs —
    /// the countdown starts over instead of a second copy piling up (M-019).
    /// </summary>
    public void Apply(UnitEffect effect)
    {
        if (effect == null)
        {
            return;
        }

        var existing = Find(effect.Key);

        if (existing != null)
        {
            existing.Refresh(effect.Source);
            return;
        }

        _effects.Add(effect);
        effect.OnApplied(gameObject);
    }

    public bool Has(object key)
    {
        return Find(key) != null;
    }

    public void Remove(object key)
    {
        var effect = Find(key);

        if (effect == null)
        {
            return;
        }

        _effects.Remove(effect);
        effect.OnRemoved(gameObject);
    }

    private UnitEffect Find(object key)
    {
        foreach (var effect in _effects)
        {
            if (ReferenceEquals(effect.Key, key))
            {
                return effect;
            }
        }

        return null;
    }

    private void Update()
    {
        // Backwards on purpose: an effect that runs out is dropped mid-walk.
        for (var i = _effects.Count - 1; i >= 0; i--)
        {
            var effect = _effects[i];

            effect.Tick(gameObject, Time.deltaTime);

            if (effect.IsExpired)
            {
                _effects.RemoveAt(i);
                effect.OnRemoved(gameObject);
            }
        }
    }

    private void OnDestroy()
    {
        // The unit died: effects go away for that reason too, and something like
        // a stun has to take its behaviour off with it (M-019).
        foreach (var effect in _effects)
        {
            effect.OnRemoved(gameObject);
        }

        _effects.Clear();
    }
}
