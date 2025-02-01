using Assets.Scripts.GameObjects.UnitBehaviour;
using Assets.Scripts.Infrastructure.Enums;
using Assets.Scripts.Infrastructure.Events;
using Assets.Scripts.Infrastructure.Helpers;
using System;
using UnityEngine;

public class BuildingBehaviour : UnitBehaviourBase
{
    private NavMeshMovement _navmeshMovement;
    private UnitEventManager _unitEventManager;
    private TeamMember _teamMember;
    private BuildingController _buildingController;

    private BuildActionStartedEventArgs actionArgs;
    private float _buildingSize = 0f;

    public void Awake()
    {
        _navmeshMovement = gameObject.GetComponent<NavMeshMovement>();
        _unitEventManager = GetComponent<UnitEventManager>();
        _teamMember = GetComponent<TeamMember>();
        _buildingController = GameObject.FindGameObjectWithTag(Tag.PlayerController.ToString())
            .GetComponent<BuildingController>();
    }

    public override void StartAction(EventArgs args)
    {
        EnableTriggerEndEvent();

        actionArgs = args as BuildActionStartedEventArgs;

        _buildingSize = actionArgs.Building.GetComponent<BuildingValues>().ObstacleSize;

        var point = gameObject.GetClosestPointToInteract(actionArgs.Point, _buildingSize);

        _navmeshMovement.Go(point);
    }

    protected override void UpdateAction()
    {
        var point = gameObject.GetClosestPointToInteract(actionArgs.Point, _buildingSize);
        var differenceVector = point - transform.position;
        differenceVector.y = 0;
        if (differenceVector.magnitude <= _navmeshMovement.StoppingDistance)
        {
            _navmeshMovement.Stop();

            _buildingController.Build(actionArgs.Point, actionArgs.Building, _teamMember.TeamId, gameObject);

            IsActive = false;
            if (TriggerEndEventFlag)
            {
                _unitEventManager.OnMoveActionEnded();
            }
        }
        else
        {
            _navmeshMovement.Go(point);
        }
    }
}
