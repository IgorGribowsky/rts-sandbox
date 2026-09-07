using Assets.Scripts.Infrastructure.Enums;
using Assets.Scripts.Infrastructure.Extensions;
using Assets.SkillsSection.Scripts;
using Assets.SkillsSection.Scripts.Events;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Assets.SkillsSection.Scripts.UnitSkills;

public class TestScenarios : MonoBehaviour
{
    public KeyCode AllEnemiesAttackUsKeyCode;
    public KeyCode WeAttackAllEnemiesKeyCode;

    [Tooltip("The nearest enemy caster stuns the unit you have selected. There is no " +
             "other way to see a stun from the inside: the target filter of every " +
             "skill only lets enemies through, so we can never stun our own (T-026).")]
    public KeyCode EnemyStunsSelectedKeyCode;

    private TeamController _teamController;

    private UnitsController _unitsController;

    private int _playerTeamId;

    void Start()
    {
        _teamController = GameObject.FindGameObjectWithTag(Tag.GameController.ToString())
            .GetComponent<TeamController>();

        var playerController = GameObject.FindGameObjectWithTag(Tag.PlayerController.ToString());

        _playerTeamId = playerController.GetComponent<PlayerTeamMember>().TeamId;
        _unitsController = playerController.GetComponent<UnitsController>();
    }

    void Update()
    {
        if (Input.GetKeyDown(AllEnemiesAttackUsKeyCode))
        {
            AllEnemiesAttackUs();
        }

        if (Input.GetKeyDown(WeAttackAllEnemiesKeyCode))
        {
            WeAttackAllEnemies();
        }

        if (Input.GetKeyDown(EnemyStunsSelectedKeyCode))
        {
            EnemyStunsSelected();
        }
    }

    public void AllEnemiesAttackUs()
    {
        var allUnitsWithTeams = GameObject.FindGameObjectsWithTag(Tag.Unit.ToString())
            .Where(o => o.GetComponent<TeamMember>() != null)
            .ToList();

        var unitTeamsDict = allUnitsWithTeams.ToDictionary(t => t.GetInstanceID(), t => t.GetComponent<TeamMember>().TeamId);

        var enemyTeamIds = _teamController.GetEnemyTeams(_playerTeamId);

        var playerUnits = allUnitsWithTeams.Where(t => unitTeamsDict[t.GetInstanceID()] == _playerTeamId).ToList();

        var enemiesUnits = allUnitsWithTeams.Where(t => enemyTeamIds.Contains(unitTeamsDict[t.GetInstanceID()]));

        foreach (var enemyUnit in enemiesUnits)
        {
            var eventManager = enemyUnit.GetComponent<UnitEventManager>();

            playerUnits.Shuffle();

            var addToQueue = false;

            foreach (var playerUnit in playerUnits)
            {
                eventManager.OnAttackCommandReceived(playerUnit, addToQueue);
                addToQueue = true;
            }
        }
    }

    /// <summary>
    /// Orders the nearest enemy caster to throw its stun at the unit the player has
    /// selected. Exists because a stun can never be aimed at our own unit by hand:
    /// SkillTargetFilter lets only enemies through. Without this there is no way to
    /// see what a stun does to a unit you are trying to order around (T-026).
    /// </summary>
    public void EnemyStunsSelected()
    {
        var target = GetSelectedPlayerUnit();

        if (target == null)
        {
            Debug.LogWarning("Select one of your units first: it is the one that gets stunned.");
            return;
        }

        var caster = FindNearestEnemyStunCaster(target, out var stunSkill);

        if (caster == null)
        {
            Debug.LogWarning("No enemy unit around carries a skill that stuns a target.");
            return;
        }

        caster.GetComponent<UnitEventManager>()
            .OnSkillCastCommandReceived(new SkillCastToTargetCommandReceivedEventArgs(stunSkill, target));
    }

    private GameObject GetSelectedPlayerUnit()
    {
        if (_unitsController == null || _unitsController.SelectedUnitsTeamId != _playerTeamId)
        {
            return null;
        }

        return _unitsController.SelectedUnits.FirstOrDefault(unit => unit != null);
    }

    /// <summary>
    /// The enemy closest to the victim that has a stunning skill, plus the skill
    /// itself — the caster walks up on its own, so distance only decides who goes.
    /// </summary>
    private GameObject FindNearestEnemyStunCaster(GameObject target, out UnitSkill stunSkill)
    {
        stunSkill = null;

        var enemyTeamIds = _teamController.GetEnemyTeams(_playerTeamId);

        GameObject nearest = null;
        var nearestDistance = float.MaxValue;

        foreach (var unit in GameObject.FindGameObjectsWithTag(Tag.Unit.ToString()))
        {
            var teamMember = unit.GetComponent<TeamMember>();

            if (teamMember == null || !enemyTeamIds.Contains(teamMember.TeamId))
            {
                continue;
            }

            var skills = unit.GetComponent<UnitSkills>();
            var skill = skills == null ? null : FindStunSkill(skills);

            if (skill == null)
            {
                continue;
            }

            var distance = (unit.transform.position - target.transform.position).sqrMagnitude;

            if (distance >= nearestDistance)
            {
                continue;
            }

            nearestDistance = distance;
            nearest = unit;
            stunSkill = skill;
        }

        return nearest;
    }

    /// <summary>
    /// The skill is found by what it does — a cast at a unit carrying a stun impact —
    /// and not by a hardcoded key, so renaming or rebinding the ability does not
    /// quietly break this scenario.
    /// </summary>
    private static UnitSkill FindStunSkill(UnitSkills skills)
    {
        if (skills.Skills == null)
        {
            return null;
        }

        foreach (var slot in skills.Skills)
        {
            if (slot?.Skill is not ActiveSkill active || active.Action is not CastToTargetAction)
            {
                continue;
            }

            if (!CarriesStun(active.Action.Impacts))
            {
                continue;
            }

            var runtimeSkill = skills.GetSkillByKeycode(slot.Keycode);

            if (runtimeSkill != null)
            {
                return runtimeSkill;
            }
        }

        return null;
    }

    private static bool CarriesStun(List<SkillImpact> impacts)
    {
        return impacts != null && impacts.Any(impact => impact is StunImpact);
    }

    public void WeAttackAllEnemies()
    {
        var allUnitsWithTeams = GameObject.FindGameObjectsWithTag(Tag.Unit.ToString())
            .Where(o => o.GetComponent<TeamMember>() != null)
            .ToList();

        var unitTeamsDict = allUnitsWithTeams.ToDictionary(t => t.GetInstanceID(), t => t.GetComponent<TeamMember>().TeamId);

        var enemyTeamIds = _teamController.GetEnemyTeams(_playerTeamId);

        var playerUnits = allUnitsWithTeams.Where(t => unitTeamsDict[t.GetInstanceID()] == _playerTeamId).ToList();

        var enemiesUnits = allUnitsWithTeams.Where(t => enemyTeamIds.Contains(unitTeamsDict[t.GetInstanceID()])).ToList();

        foreach (var playerUnit in playerUnits)
        {
            var eventManager = playerUnit.GetComponent<UnitEventManager>();

            enemiesUnits.Shuffle();

            var addToQueue = false;

            foreach (var enemyUnit in enemiesUnits)
            {
                eventManager.OnAttackCommandReceived(enemyUnit, addToQueue);
                addToQueue = true;
            }
        }
    }
}
