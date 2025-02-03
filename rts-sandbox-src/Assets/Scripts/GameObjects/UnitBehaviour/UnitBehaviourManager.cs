using Assets.Scripts.Infrastructure.Events;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.GameObjects.UnitBehaviour
{
    public class UnitBehaviourManager : MonoBehaviour
    {
        private UnitEventManager _unitEventManager;

        private List<UnitBehaviourBase> UnitBehaviourCases = new List<UnitBehaviourBase>();
        private MovementBehaviour _movementBehaviour;
        private FollowingBehaviour _followingBehaviour;
        private MeleeAttackingBehaviour _meleeAttackingBehaviour;
        private RangeAttackingBehaviour _rangeAttackingBehaviour;
        private AMovementBehaviour _aMovementBehaviour;
        private AutoAttackIdleBehaviour _autoAttackIdleBehaviour;
        private AutoAttackBuildingBehaviour _autoAttackBuildingBehaviour;
        private HoldingBehaviour _holdingBehaviour;
        private BuildingBehaviour _buildingBehaviour;

        public void Awake()
        {
            _unitEventManager = GetComponent<UnitEventManager>();

            _movementBehaviour = GetComponent<MovementBehaviour>();
            if (_movementBehaviour != null)
            {
                UnitBehaviourCases.Add(_movementBehaviour);
                _unitEventManager.MoveActionStarted += StartMovementBehaviour;
            }

            _followingBehaviour = GetComponent<FollowingBehaviour>();
            if (_followingBehaviour != null)
            {
                UnitBehaviourCases.Add(_followingBehaviour);
                _unitEventManager.FollowActionStarted += StartFollowingBehaviour;
            }

            _rangeAttackingBehaviour = GetComponent<RangeAttackingBehaviour>();
            if (_rangeAttackingBehaviour != null)
            {
                UnitBehaviourCases.Add(_rangeAttackingBehaviour);
                _unitEventManager.AttackActionStarted += StartRangeAttackingBehaviour;
            }

            _meleeAttackingBehaviour = GetComponent<MeleeAttackingBehaviour>();
            if (_meleeAttackingBehaviour != null)
            {
                UnitBehaviourCases.Add(_meleeAttackingBehaviour);
                _unitEventManager.AttackActionStarted += StartMeleeAttackingBehaviour;
            }

            _aMovementBehaviour = GetComponent<AMovementBehaviour>();
            if (_aMovementBehaviour != null)
            {
                UnitBehaviourCases.Add(_aMovementBehaviour);
                _unitEventManager.AMoveActionStarted += StartAMovementBehaviour;
            }

            _autoAttackIdleBehaviour = GetComponent<AutoAttackIdleBehaviour>();
            if (_autoAttackIdleBehaviour != null)
            {
                UnitBehaviourCases.Add(_autoAttackIdleBehaviour);
                _unitEventManager.AutoAttackIdleStarted += StartAutoAttackIdle;
            }

            _autoAttackBuildingBehaviour = GetComponent<AutoAttackBuildingBehaviour>();
            if (_autoAttackBuildingBehaviour != null)
            {
                UnitBehaviourCases.Add(_autoAttackBuildingBehaviour);
                _unitEventManager.AutoAttackIdleStarted += StartAutoAttackBuilding;
            }

            _holdingBehaviour = GetComponent<HoldingBehaviour>();
            if (_holdingBehaviour != null)
            {
                UnitBehaviourCases.Add(_holdingBehaviour);
                _unitEventManager.HoldActionStarted += StartHoldingBehaviour;
            }

            _buildingBehaviour = GetComponent<BuildingBehaviour>();
            if (_buildingBehaviour != null)
            {
                UnitBehaviourCases.Add(_buildingBehaviour);
                _unitEventManager.BuildActionStarted += StartBuildingBehaviour;
            }
        }

        private void StartMovementBehaviour(MoveActionStartedEventArgs args)
        {
            UnitBehaviourCases.ForEach(x => x.IsActive = false);
            _movementBehaviour.IsActive = true;
            _movementBehaviour.StartAction(args);
        }

        private void StartFollowingBehaviour(FollowActionStartedEventArgs args)
        {
            UnitBehaviourCases.ForEach(x => x.IsActive = false);
            _followingBehaviour.IsActive = true;
            _followingBehaviour.StartAction(args);
        }

        private void StartRangeAttackingBehaviour(AttackActionStartedEventArgs args)
        {
            UnitBehaviourCases.ForEach(x => x.IsActive = false);
            _rangeAttackingBehaviour.IsActive = true;
            _rangeAttackingBehaviour.StartAction(args);
        }

        private void StartMeleeAttackingBehaviour(AttackActionStartedEventArgs args)
        {
            UnitBehaviourCases.ForEach(x => x.IsActive = false);
            _meleeAttackingBehaviour.IsActive = true;
            _meleeAttackingBehaviour.StartAction(args);
        }

        private void StartAMovementBehaviour(MoveActionStartedEventArgs args)
        {
            UnitBehaviourCases.ForEach(x => x.IsActive = false);
            _aMovementBehaviour.IsActive = true;
            _aMovementBehaviour.StartAction(args);
        }

        private void StartAutoAttackIdle(AutoAttackIdleStartedEventArgs args)
        {
            UnitBehaviourCases.ForEach(x => x.IsActive = false);
            _autoAttackIdleBehaviour.IsActive = true;
            _autoAttackIdleBehaviour.StartAction(new MoveActionStartedEventArgs(args.MovePoint));
        }

        private void StartAutoAttackBuilding(AutoAttackIdleStartedEventArgs args)
        {
            UnitBehaviourCases.ForEach(x => x.IsActive = false);
            _autoAttackBuildingBehaviour.IsActive = true;
            _autoAttackBuildingBehaviour.StartAction(new MoveActionStartedEventArgs(args.MovePoint));
        }


        private void StartHoldingBehaviour(HoldActionStartedEventArgs args)
        {
            UnitBehaviourCases.ForEach(x => x.IsActive = false);
            _holdingBehaviour.IsActive = true;
            _holdingBehaviour.StartAction(args);
        }

        private void StartBuildingBehaviour(BuildActionStartedEventArgs args)
        {
            UnitBehaviourCases.ForEach(x => x.IsActive = false);
            _buildingBehaviour.IsActive = true;
            _buildingBehaviour.StartAction(args);
        }
    }
}