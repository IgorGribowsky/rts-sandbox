using Assets.Scripts;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TeamController : MonoBehaviour
{
    public List<Team> Teams;

    public List<Alliance> Alliances;

    void Awake()
    {
        GameServices.TeamController = this;
        InvalidateTeamCache();
    }

    /// <summary>
    /// Who is on whose side never changes while the game runs, and auto attack
    /// asked for it every frame for every unit — four LINQ passes and as many
    /// new lists each time. Now it is worked out once per team (T-011).
    /// Call <see cref="InvalidateTeamCache"/> if alliances ever start changing.
    /// </summary>
    private readonly Dictionary<int, List<int>> _allyCache = new Dictionary<int, List<int>>();
    private readonly Dictionary<int, List<int>> _enemyCache = new Dictionary<int, List<int>>();

    /// <summary>Read only: the same list is handed to every caller.</summary>
    public List<int> GetAllyTeams(int targetTeamId)
    {
        List<int> cached;
        if (_allyCache.TryGetValue(targetTeamId, out cached))
        {
            return cached;
        }

        cached = Alliances
            .Where(a => a.TeamIds.Contains(targetTeamId))
            .SelectMany(a => a.TeamIds)
            .Distinct()
            .ToList();

        _allyCache[targetTeamId] = cached;

        return cached;
    }

    /// <summary>Read only: the same list is handed to every caller.</summary>
    public List<int> GetEnemyTeams(int targetTeamId)
    {
        List<int> cached;
        if (_enemyCache.TryGetValue(targetTeamId, out cached))
        {
            return cached;
        }

        var allyTeams = GetAllyTeams(targetTeamId);

        cached = Teams
            .Where(t => !allyTeams.Contains(t.Id))
            .Select(t => t.Id)
            .ToList();

        _enemyCache[targetTeamId] = cached;

        return cached;
    }

    public void InvalidateTeamCache()
    {
        _allyCache.Clear();
        _enemyCache.Clear();
    }
}

[Serializable]
public class Team
{
    public int Id;

    public string Name;

    public Color Color;

    public bool IsNeutral = false;
}

[Serializable]
public class Alliance
{
    public List<int> TeamIds;
}