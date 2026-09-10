using Assets.Scripts;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Who is on whose side.
///
/// Two things in this project are called "neutral" and they are not the same
/// (T-018):
///
/// * a **peaceful** team is one with <see cref="Team.IsNeutral"/> — a mine, a
///   grove, anything standing on the map that must not take part in a fight.
///   It is an ally to everyone and an enemy to nobody, whatever the alliances
///   say;
/// * a team that is **against everyone** is an ordinary team simply left out
///   of every alliance. It is an ally only to itself and an enemy to all the
///   rest — except the peaceful ones.
/// </summary>
public class TeamController : MonoBehaviour
{
    public List<Team> Teams;

    public List<Alliance> Alliances;

    /// <summary>
    /// Who is on whose side never changes while the game runs, and auto attack
    /// asked for it every frame for every unit — four LINQ passes and as many
    /// new lists each time. Now it is worked out once per team (T-011).
    /// Call <see cref="InvalidateTeamCache"/> if alliances ever start changing.
    /// </summary>
    private readonly Dictionary<int, List<int>> _allyCache = new Dictionary<int, List<int>>();
    private readonly Dictionary<int, List<int>> _enemyCache = new Dictionary<int, List<int>>();

    void Awake()
    {
        GameServices.TeamController = this;
        InvalidateTeamCache();
    }

    /// <summary>A peaceful team: an ally to everyone, an enemy to nobody.</summary>
    public bool IsPeaceful(int teamId)
    {
        var team = Teams.FirstOrDefault(t => t.Id == teamId);

        return team != null && team.IsNeutral;
    }

    /// <summary>Read only: the same list is handed to every caller.</summary>
    public List<int> GetAllyTeams(int targetTeamId)
    {
        List<int> cached;
        if (_allyCache.TryGetValue(targetTeamId, out cached))
        {
            return cached;
        }

        if (IsPeaceful(targetTeamId))
        {
            // Everyone, without so much as a glance at the alliances.
            cached = Teams.Select(t => t.Id).Distinct().ToList();
            if (!cached.Contains(targetTeamId))
            {
                cached.Add(targetTeamId);
            }

            _allyCache[targetTeamId] = cached;

            return cached;
        }

        var allies = Alliances
            .Where(a => a.TeamIds.Contains(targetTeamId))
            .SelectMany(a => a.TeamIds)
            .ToList();

        // A team in no alliance at all is its own alliance of one. Without this
        // its list of allies came out empty and it counted itself an enemy.
        allies.Add(targetTeamId);

        // The peaceful ones are everybody's allies, so they belong here too.
        allies.AddRange(Teams.Where(t => t.IsNeutral).Select(t => t.Id));

        cached = allies.Distinct().ToList();
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

        if (IsPeaceful(targetTeamId))
        {
            // A peaceful team attacks nobody.
            cached = new List<int>();
            _enemyCache[targetTeamId] = cached;

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

    [Tooltip("Peaceful: an ally to everyone, an enemy to nobody. Not the same " +
             "as a team against everyone — that one is simply in no alliance.")]
    public bool IsNeutral = false;
}

[Serializable]
public class Alliance
{
    public List<int> TeamIds;
}
