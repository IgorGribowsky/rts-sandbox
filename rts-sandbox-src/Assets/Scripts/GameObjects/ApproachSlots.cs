using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Places around one object for the units that walk up to it (M-006). A unit
/// takes a free point on the circle around the target and holds it while it
/// walks there and while it stands there, so two units never aim at the same
/// spot and nobody shoves for it. Shared by cutting, handing in at a storage
/// (T-063) and melee (T-064).
///
/// Added to the target on first use and destroyed with it. A unit that died
/// drops out by itself: its reservation is skipped as soon as it is null.
/// </summary>
public class ApproachSlots : MonoBehaviour
{
    // Neighbours on the circle stand this much wider apart than their bodies.
    private const float SpacingFactor = 1.1f;

    private struct Reservation
    {
        public GameObject Unit;
        public Vector3 Point;
        public float Radius;
    }

    private readonly List<Reservation> _taken = new List<Reservation>();

    public static ApproachSlots Of(GameObject target)
    {
        var slots = target.GetComponent<ApproachSlots>();
        return slots != null ? slots : target.AddComponent<ApproachSlots>();
    }

    /// <summary>How many places are held around the target, without adding the component.</summary>
    public static int TakenCountOf(GameObject target)
    {
        var slots = target.GetComponent<ApproachSlots>();
        return slots != null ? slots.TakenCount : 0;
    }

    public int TakenCount
    {
        get
        {
            Prune();
            return _taken.Count;
        }
    }

    public bool IsHeldBy(GameObject unit, out Vector3 point)
    {
        Prune();

        foreach (var reservation in _taken)
        {
            if (reservation.Unit == unit)
            {
                point = reservation.Point;
                return true;
            }
        }

        point = default;
        return false;
    }

    /// <summary>
    /// Takes the free place nearest to the side the unit comes from. False when
    /// maxUnits are already here or the circle has no free reachable point —
    /// the caller decides whether to look elsewhere or to push in the old way.
    /// </summary>
    /// <param name="ringRadius">From the target's center to the unit's center.</param>
    /// <param name="maxUnits">0 — as many as fit on the circle.</param>
    public bool TryTake(GameObject unit, Vector3 center, float ringRadius, float unitRadius,
        int agentTypeId, int maxUnits, out Vector3 point)
    {
        Release(unit);
        Prune();

        point = default;

        if (maxUnits > 0 && _taken.Count >= maxUnits)
        {
            return false;
        }

        var toUnit = unit.transform.position - center;
        toUnit.y = 0f;
        var baseAngle = toUnit.sqrMagnitude > 0.0001f ? Mathf.Atan2(toUnit.z, toUnit.x) : 0f;

        var count = Mathf.Max(1, Mathf.FloorToInt(2f * Mathf.PI * ringRadius / (2f * unitRadius * SpacingFactor)));
        var step = 2f * Mathf.PI / count;
        var filter = new NavMeshQueryFilter { agentTypeID = agentTypeId, areaMask = NavMesh.AllAreas };

        for (var i = 0; i < count; i++)
        {
            // 0, +1, -1, +2, -2... steps away from the side the unit comes from.
            var offset = (i + 1) / 2 * (i % 2 == 1 ? 1 : -1);
            var angle = baseAngle + offset * step;
            var candidate = center + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * ringRadius;

            if (!NavMesh.SamplePosition(candidate, out var hit, 2f, filter))
            {
                continue;
            }

            // The point may sit in a hole carved by a building or a tree: the
            // nearest walkable spot is fine if it is not a body away from it.
            var shift = hit.position - candidate;
            shift.y = 0f;
            if (shift.magnitude > unitRadius || IsTaken(hit.position, unitRadius))
            {
                continue;
            }

            point = hit.position;
            _taken.Add(new Reservation { Unit = unit, Point = point, Radius = unitRadius });
            return true;
        }

        return false;
    }

    public void Release(GameObject unit)
    {
        for (var i = _taken.Count - 1; i >= 0; i--)
        {
            if (_taken[i].Unit == unit)
            {
                _taken.RemoveAt(i);
            }
        }
    }

    private bool IsTaken(Vector3 point, float radius)
    {
        foreach (var reservation in _taken)
        {
            var between = reservation.Point - point;
            between.y = 0f;

            if (between.magnitude < (radius + reservation.Radius) * 0.95f)
            {
                return true;
            }
        }

        return false;
    }

    private void Prune()
    {
        _taken.RemoveAll(reservation => reservation.Unit == null);
    }
}
