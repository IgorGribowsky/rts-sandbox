using Assets.Scripts.Infrastructure.Extensions;
using System.Linq;
using UnityEngine;

public class TestScenarios : MonoBehaviour
{
    public KeyCode AllEnemiesAttackUsKeyCode;
    public KeyCode WeAttackAllEnemiesKeyCode;

    private TeamController _teamController;

    private int _playerTeamId;

    void Start()
    {
        _teamController = GameObject.FindGameObjectWithTag("GameController")
            .GetComponent<TeamController>();

        _playerTeamId = GameObject.FindGameObjectWithTag("PlayerController")
            .GetComponent<PlayerTeamMember>().TeamId;
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
    }

    public void AllEnemiesAttackUs()
    {
        var allUnitsWithTeams = GameObject.FindGameObjectsWithTag("Unit")
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

    public void WeAttackAllEnemies()
    {
        var allUnitsWithTeams = GameObject.FindGameObjectsWithTag("Unit")
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
