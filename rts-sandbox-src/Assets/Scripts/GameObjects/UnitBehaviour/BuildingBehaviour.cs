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
    private BuildingGridController _buildingGridController;

    private BuildActionStartedEventArgs actionArgs;
    private int _buildingSize = 0;

    public void Awake()
    {
        _navmeshMovement = gameObject.GetComponent<NavMeshMovement>();
        _unitEventManager = GetComponent<UnitEventManager>();
        _teamMember = GetComponent<TeamMember>();
        _buildingController = GameObject.FindGameObjectWithTag(Tag.PlayerController.ToString())
            .GetComponent<BuildingController>();
        _buildingGridController = GameObject.FindGameObjectWithTag(Tag.PlayerController.ToString())
            .GetComponent<BuildingGridController>();
    }

    public override void StartAction(EventArgs args)
    {
        EnableTriggerEndEvent();

        actionArgs = args as BuildActionStartedEventArgs;

        var buildingValues = actionArgs.Building.GetComponent<BuildingValues>();
        _buildingSize = buildingValues.ObstacleSize;

        if (!_buildingGridController.CheckIfCanBuildAt(actionArgs.Point, _buildingSize, gameObject) && !buildingValues.IsHeldMine)
        {
            Debug.Log("Can't build here!");

            IsActive = false;

            if (TriggerEndEventFlag)
            {
                _unitEventManager.OnBuildActionEnded();
            }

            return;
        }

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

            _buildingController.Build(actionArgs, _teamMember.TeamId, gameObject);

            IsActive = false;
            if (TriggerEndEventFlag)
            {
                _unitEventManager.OnBuildActionEnded();
            }
        }
        else
        {
            _navmeshMovement.Go(point);
        }
    }
}
