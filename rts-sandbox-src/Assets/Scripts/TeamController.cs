using System;
using System.Collections.Generic;
using UnityEngine;

public class TeamController : MonoBehaviour
{
    public List<Team> Teams;

    void Start()
    {
        
    }

    void Update()
    {
        
    }
}

[Serializable]
public class Team
{
    public int Id;

    public string Name;

    public Color Color;
}