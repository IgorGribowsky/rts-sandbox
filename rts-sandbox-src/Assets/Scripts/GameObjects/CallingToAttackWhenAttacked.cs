using Assets.Scripts.Infrastructure.Constants;
using Assets.Scripts.Infrastructure.Events;
using Assets.Scripts.Infrastructure.Helpers;
using UnityEngine;

public class CallingToAttackWhenAttacked : MonoBehaviour
{
    private UnitEventManager _unitEventManager;
    private TeamMember _teamMember;
    private TeamController _teamController;

    void Awake()
    {
        _unitEventManager = GetComponent<UnitEventManager>();
        _teamMember = GetComponent<TeamMember>();
        _teamController = GameObject.FindGameObjectWithTag("GameController")
            .GetComponent<TeamController>();
        _unitEventManager.DamageReceived += OnDamageReceivedHandler;
    }

    public void OnDamageReceivedHandler(DamageReceivedEventArgs args)
    {
        if (gameObject.GetDistanceTo(args.Attacker) > GameConstants.DamageReceivedAgressionDistance)
        {
            return;
        }

        var allyTeamIds = _teamController.GetAllyTeams(_teamMember.TeamId);

        var unitsToCall = gameObject.GetAllUnitsInRadius(GameConstants.DamageReceivedCallToAttackDistance, unit =>
        {
            var teamMember = unit.GetComponent<TeamMember>();
            return teamMember != null && allyTeamIds.Contains(teamMember.TeamId);
        });

        foreach (var unit in unitsToCall)
        {
            unit.GetComponent<UnitEventManager>()?.OnCalledToAttack(gameObject, args.Attacker);
        }
    }
}
