using Assets.Scripts;
using Assets.Scripts.Infrastructure.Events;
using RtsSandbox.Rules;
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Experience and level of one unit (M-026). Only types with levels get it —
/// now the Caster alone. The level changes nothing yet.
///
/// An enemy dies: every unit with levels of the other side within the share
/// radius gets an equal part of its reward; a unit on the top level is not
/// counted. Who dealt the last blow does not matter (Q-12, B).
/// </summary>
public class UnitExperience : MonoBehaviour
{
    private static readonly List<UnitExperience> _all = new List<UnitExperience>();

    public ExperienceTable Table;

    [SerializeField] private int _level = ExperienceRules.FirstLevel;
    [SerializeField] private int _experience;

    private TeamMember _team;
    private UnitValues _values;

    /// <summary>Level or experience changed.</summary>
    public event Action Changed;

    public int Level => _level;

    /// <summary>Gathered inside the current level.</summary>
    public int Experience => _experience;

    /// <summary>What the current level costs to leave; 0 on the top level.</summary>
    public int ExperienceToNext => ExperienceRules.CostOfNextLevel(_level, LevelUpCosts);

    public bool IsMaxLevel => _level >= ExperienceRules.MaxLevel(LevelUpCosts);

    private IReadOnlyList<int> LevelUpCosts => Table != null ? Table.LevelUpCosts : (IReadOnlyList<int>)Array.Empty<int>();

    private void Awake()
    {
        _team = GetComponent<TeamMember>();
        _values = GetComponent<UnitValues>();
    }

    private void OnEnable()
    {
        _all.Add(this);
        UnitEventManager.AnyUnitDied += OnAnyUnitDied;
    }

    private void OnDisable()
    {
        _all.Remove(this);
        UnitEventManager.AnyUnitDied -= OnAnyUnitDied;
    }

    /// <summary>Adds experience straight away, past the sharing. For tools and tests.</summary>
    public void Add(int amount)
    {
        var state = ExperienceRules.Add(new LevelState(_level, _experience), amount, LevelUpCosts);
        if (state.Level == _level && state.Experience == _experience)
        {
            return;
        }

        _level = state.Level;
        _experience = state.Experience;
        Changed?.Invoke();
    }

    private void OnAnyUnitDied(DiedEventArgs args)
    {
        var dead = args.Dead;
        if (dead == null || dead == gameObject || Table == null || !IsReceiving(this, dead))
        {
            return;
        }

        var reward = dead.GetComponent<UnitValues>()?.Type?.ExperienceReward ?? 0;
        if (reward <= 0)
        {
            return;
        }

        // Every receiver counts the others the same way, so each one adds just
        // its own part and nobody has to hand the shares out.
        var receivers = 0;
        foreach (var other in _all)
        {
            if (other._team != null && _team != null && other._team.TeamId == _team.TeamId
                && IsReceiving(other, dead))
            {
                receivers++;
            }
        }

        Add(ExperienceRules.Share(reward, receivers));
    }

    private static bool IsReceiving(UnitExperience unit, GameObject dead)
    {
        if (unit.IsMaxLevel || unit.Table == null || unit._team == null)
        {
            return false;
        }

        if (unit._values != null && unit._values.CurrentHp <= 0f)
        {
            return false;
        }

        var deadTeam = dead.GetComponent<TeamMember>();
        if (deadTeam == null || IsAlly(unit._team.TeamId, deadTeam.TeamId))
        {
            return false;
        }

        var offset = dead.transform.position - unit.transform.position;
        offset.y = 0f;
        return offset.magnitude <= unit.Table.ShareRadius;
    }

    private static bool IsAlly(int teamId, int otherTeamId)
    {
        if (teamId == otherTeamId)
        {
            return true;
        }

        var teams = GameServices.TeamController;
        return teams != null && teams.GetAllyTeams(teamId).Contains(otherTeamId);
    }
}
