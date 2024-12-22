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
        }

        private void StartMovementBehaviour(MoveActionStartedEventArgs args)
        {
            UnitBehaviourCases.ForEach(x => x.IsActive = false);
            _movementBehaviour.StartAction(args);
            _movementBehaviour.IsActive = true;
        }

        private void StartFollowingBehaviour(FollowActionStartedEventArgs args)
        {
            UnitBehaviourCases.ForEach(x => x.IsActive = false);
            _followingBehaviour.StartAction(args);
            _followingBehaviour.IsActive = true;
        }

        private void StartRangeAttackingBehaviour(AttackActionStartedEventArgs args)
        {
            UnitBehaviourCases.ForEach(x => x.IsActive = false);
            _rangeAttackingBehaviour.StartAction(args);
            _rangeAttackingBehaviour.IsActive = true;
        }

        private void StartMeleeAttackingBehaviour(AttackActionStartedEventArgs args)
        {
            UnitBehaviourCases.ForEach(x => x.IsActive = false);
            _meleeAttackingBehaviour.StartAction(args);
            _meleeAttackingBehaviour.IsActive = true;
        }

        private void StartAMovementBehaviour(MoveActionStartedEventArgs args)
        {
            UnitBehaviourCases.ForEach(x => x.IsActive = false);
            _aMovementBehaviour.StartAction(args);
            _aMovementBehaviour.IsActive = true;
        }
    }
}