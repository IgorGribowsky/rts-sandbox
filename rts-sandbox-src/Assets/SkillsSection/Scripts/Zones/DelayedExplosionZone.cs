using Assets.Scripts.Infrastructure.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// The blast and the zone left behind it, as one object on the ground. It holds
/// the whole life of the thing: wait out the delay, hit everybody in the circle,
/// then keep hitting whoever stands inside until the time runs out.
///
/// A zone and not an effect on the units (M-019) for one reason: a zone has to
/// STOP hitting somebody who walked out of it, and an effect hangs on its target
/// until it expires. So every tick the zone asks who is inside right now.
///
/// It lives on its own, so the caster is free to die: the blast still happens.
/// That is also why the owner's team is kept as a number — the owner's
/// TeamMember may be gone by the time the zone ticks.
///
/// TODO(H-002): the prefab wears a flattened cube with the building-grid
/// material as a placeholder, replace with a real mark on the ground.
/// </summary>
public class DelayedExplosionZone : MonoBehaviour
{
    /// <summary>A tick rate of zero would hang the tick loop below.</summary>
    private const float MinTickRate = 0.05f;

    private GameObject _owner;

    private int _ownerTeamId;

    private TargetType _targetType;

    private float _radius;

    private Action<GameObject, GameObject> _blast;

    private Action<GameObject, GameObject> _tick;

    private float _untilBlast;

    private float _zoneLeft;

    private float _tickRate;

    private float _untilNextTick;

    private bool _blasted;

    private bool _started;

    /// <summary>Not called Start: that name belongs to Unity's own message.</summary>
    public void StartZone(GameObject owner, int ownerTeamId, TargetType targetType, float radius,
        float delay, float zoneDuration, float tickRate,
        Action<GameObject, GameObject> blast, Action<GameObject, GameObject> tick)
    {
        _owner = owner;
        _ownerTeamId = ownerTeamId;
        _targetType = targetType;
        _radius = radius;
        _untilBlast = delay;
        _zoneLeft = zoneDuration;
        _tickRate = Mathf.Max(tickRate, MinTickRate);
        _untilNextTick = _tickRate;
        _blast = blast;
        _tick = tick;
        _started = true;

        // The mark on the ground shows exactly what the circle covers: the flat
        // primitive is one unit wide, so twice the radius spans it.
        var scale = transform.localScale;
        transform.localScale = new Vector3(_radius * 2f, scale.y, _radius * 2f);
    }

    private void Update()
    {
        if (!_started)
        {
            return;
        }

        if (!_blasted)
        {
            _untilBlast -= Time.deltaTime;

            if (_untilBlast > 0)
            {
                return;
            }

            _blasted = true;
            ApplyToEveryoneInside(_blast);
        }

        if (_zoneLeft <= 0)
        {
            Destroy(gameObject);
            return;
        }

        _zoneLeft -= Time.deltaTime;
        _untilNextTick -= Time.deltaTime;

        // A long frame can cover more than one tick, so no damage is lost.
        while (_untilNextTick <= 0)
        {
            _untilNextTick += _tickRate;
            ApplyToEveryoneInside(_tick);
        }

        if (_zoneLeft <= 0)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Who is inside is asked anew every time — that is what makes walking out of
    /// the zone work.
    /// </summary>
    private void ApplyToEveryoneInside(Action<GameObject, GameObject> apply)
    {
        if (apply == null)
        {
            return;
        }

        // Materialized before anything is applied: a hit can kill a unit, and a
        // lazy query would then be walking over a destroyed object.
        var inside = UnitsInside().ToList();

        foreach (var unit in inside)
        {
            if (unit == null)
            {
                continue;
            }

            apply(unit, _owner);
        }
    }

    /// <summary>
    /// Distance is measured from the centre of the zone to the centre of the unit,
    /// deliberately not surface to surface: the radius is a number a designer sets
    /// in the asset and it has to mean the same thing for a warrior and for a
    /// giant, and it has to match the mark drawn on the ground.
    /// </summary>
    private IEnumerable<GameObject> UnitsInside()
    {
        var center = transform.position;

        return GameObject.FindGameObjectsWithTag(Tag.Unit.ToString())
            .Where(unit => unit != null)
            .Where(unit => IsInCircle(unit.transform.position, center))
            .Where(unit => SkillTargetFilter.CanHit(unit, _ownerTeamId, _owner, _targetType));
    }

    private bool IsInCircle(Vector3 position, Vector3 center)
    {
        var delta = position - center;
        delta.y = 0;

        return delta.sqrMagnitude <= _radius * _radius;
    }
}
