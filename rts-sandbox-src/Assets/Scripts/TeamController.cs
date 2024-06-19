using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TeamController : MonoBehaviour
{
    public List<Team> Teams;

    public List<Alliance> Alliances;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public List<int> GetAllyTeams(int targetTeamId)
    {
        return Alliances
            .Where(a => a.TeamIds.Contains(targetTeamId))
            .SelectMany(a => a.TeamIds)
            .Where(id => id != targetTeamId)
            .ToList();
    }
}

[Serializable]
public class Team
{
    public int Id;

    public string Name;

    public Color Color;
}

[Serializable]
public class Alliance
{
    public List<int> TeamIds;
}