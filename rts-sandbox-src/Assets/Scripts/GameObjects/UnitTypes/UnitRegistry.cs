using Assets.Scripts.GameObjects;
using RtsSandbox.Rules;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// One unit as the target search sees it: the components it would otherwise
/// look up with GetComponent every frame, plus the size, which never changes.
/// </summary>
public sealed class UnitRecord
{
    public GameObject GameObject;
    public Transform Transform;
    public Renderer Renderer;
    public TeamMember Team;
    public UnitValues Values;

    /// <summary>
    /// Half the footprint, the same number GameObjectExtensions.GetSize used to
    /// work out from scratch on every comparison. Constant for the life of the
    /// unit: it comes from the obstacle size, the agent radius or the mesh.
    /// </summary>
    public float Size;

    /// <summary>The unit's orders, taken once. Null for what takes none.</summary>
    public UnitCommandManager Commands;

    /// <summary>The bars over the unit, taken once. Null for what has none.</summary>
    public BarsContaining Bars;

    /// <summary>
    /// Cannot walk: a building, a mine. The fog remembers such a thing where
    /// it was seen; a mine is not IsBuilding, so the agent decides.
    /// </summary>
    public bool IsStatic;

    /// <summary>
    /// What the player is shown of this unit under the fog of war. Kept by
    /// FogOfWar; Visible while there is no fog (M-027, T-071.3).
    /// </summary>
    public FogSight Sight = FogSight.Visible;

    /// <summary>The player has seen it at least once: a building is then remembered.</summary>
    public bool WasSeen;

    /// <summary>Where the player saw it last: an attack order whose target went into the fog goes there.</summary>
    public Vector3 LastSeenPosition;

    public int TeamId => Team != null ? Team.TeamId : 0;

    /// <summary>Centre of the bounds flattened onto the ground, as before.</summary>
    public Vector3 GroundCenter
    {
        get
        {
            var center = Renderer.bounds.center;
            center.y = 0f;
            return center;
        }
    }
}

/// <summary>
/// Every living unit on the map, kept in one list instead of being fished out
/// with FindGameObjectsWithTag on every search.
///
/// Why it exists: auto attack looked for a target every frame, and every look
/// walked the whole scene, calling GetComponent on both sides of every pair.
/// One pass over 97 units cost 22.5 ms — more than a whole frame at 60 FPS.
/// The registry keeps the components and the size ready, and splits the units
/// by team, so a search only walks the teams it can actually attack (T-011).
/// </summary>
public static class UnitRegistry
{
    private static readonly List<UnitRecord> _all = new List<UnitRecord>();
    private static readonly Dictionary<int, List<UnitRecord>> _byTeam = new Dictionary<int, List<UnitRecord>>();
    private static readonly Dictionary<int, UnitRecord> _byInstanceId = new Dictionary<int, UnitRecord>();

    /// <summary>Every registered unit. Read only — do not hold on to the list.</summary>
    public static IReadOnlyList<UnitRecord> All => _all;

    /// <summary>A unit has just come in. The fog decides at once whether the player may see it.</summary>
    public static event System.Action<UnitRecord> Registered;

    public static UnitRecord Register(GameObject unit)
    {
        var id = unit.GetInstanceID();
        if (_byInstanceId.ContainsKey(id))
        {
            return _byInstanceId[id];
        }

        var record = new UnitRecord
        {
            GameObject = unit,
            Transform = unit.transform,
            Renderer = unit.GetComponent<Renderer>(),
            Team = unit.GetComponent<TeamMember>(),
            Values = unit.GetComponent<UnitValues>(),
            Size = MeasureSize(unit),
            Commands = unit.GetComponent<UnitCommandManager>(),
            Bars = unit.GetComponent<BarsContaining>(),
            IsStatic = unit.GetComponent<NavMeshAgent>() == null,
        };

        _all.Add(record);
        _byInstanceId[id] = record;
        TeamList(record.TeamId).Add(record);

        Registered?.Invoke(record);

        return record;
    }

    public static void Unregister(GameObject unit)
    {
        var id = unit.GetInstanceID();
        UnitRecord record;
        if (!_byInstanceId.TryGetValue(id, out record))
        {
            return;
        }

        _byInstanceId.Remove(id);
        _all.Remove(record);
        TeamList(record.TeamId).Remove(record);
    }

    /// <summary>
    /// Play mode may start with what the last run left behind: the lists are
    /// static and domain reload can be switched off in the editor.
    /// </summary>
    public static void Clear()
    {
        _all.Clear();
        _byTeam.Clear();
        _byInstanceId.Clear();
    }

    public static List<UnitRecord> OfTeam(int teamId)
    {
        return TeamList(teamId);
    }

    /// <summary>
    /// The nearest unit of one of the given teams, no further than radius.
    /// Distance is measured exactly as before: between the flattened centres of
    /// the bounds, minus both sizes.
    /// </summary>
    public static GameObject FindNearestOfTeams(
        UnitRecord from,
        float radius,
        IList<int> teamIds,
        System.Func<UnitRecord, bool> extraFilter = null)
    {
        if (from == null || from.Renderer == null)
        {
            return null;
        }

        var fromCenter = from.GroundCenter;
        var fromSize = from.Size;

        GameObject closest = null;
        var closestDistance = radius;

        for (var t = 0; t < teamIds.Count; t++)
        {
            var team = TeamList(teamIds[t]);

            for (var i = 0; i < team.Count; i++)
            {
                var candidate = team[i];

                if (candidate == from || candidate.GameObject == null || candidate.Renderer == null)
                {
                    continue;
                }

                if (extraFilter != null && !extraFilter(candidate))
                {
                    continue;
                }

                var distance = Vector3.Distance(fromCenter, candidate.GroundCenter) - (fromSize + candidate.Size);

                if (distance < closestDistance)
                {
                    closest = candidate.GameObject;
                    closestDistance = distance;
                }
            }
        }

        return closest;
    }

    public static UnitRecord Of(GameObject unit)
    {
        UnitRecord record;
        return unit != null && _byInstanceId.TryGetValue(unit.GetInstanceID(), out record) ? record : null;
    }

    private static List<UnitRecord> TeamList(int teamId)
    {
        List<UnitRecord> list;
        if (!_byTeam.TryGetValue(teamId, out list))
        {
            list = new List<UnitRecord>();
            _byTeam[teamId] = list;
        }

        return list;
    }

    /// <summary>The same rule GameObjectExtensions.GetSize follows, worked out once.</summary>
    private static float MeasureSize(GameObject unit)
    {
        var buildingValues = unit.GetComponent<BuildingValues>();
        if (buildingValues != null)
        {
            return buildingValues.ObstacleSize / 2f;
        }

        var agent = unit.GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            return NavMesh.GetSettingsByID(agent.agentTypeID).agentRadius;
        }

        var renderer = unit.GetComponent<Renderer>();
        if (renderer == null)
        {
            return 0f;
        }

        var extents = renderer.bounds.extents;
        extents.y = 0f;

        return extents.magnitude / Mathf.Sqrt(2);
    }
}
