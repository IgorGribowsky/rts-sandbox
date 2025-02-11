using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TeamController : MonoBehaviour
{
    public List<Team> Teams;

    public List<Alliance> Alliances;

    public List<int> GetAllyTeams(int targetTeamId)
    {
        return Alliances
            .Where(a => a.TeamIds.Contains(targetTeamId))
            .SelectMany(a => a.TeamIds)
            .Distinct()
            .ToList();
    }

    public List<int> GetEnemyTeams(int targetTeamId)
    {
        var allyTeams = GetAllyTeams(targetTeamId);

        return Teams
            .Where(t => !allyTeams.Contains(t.Id))
            .Select(t => t.Id)
            .ToList();
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