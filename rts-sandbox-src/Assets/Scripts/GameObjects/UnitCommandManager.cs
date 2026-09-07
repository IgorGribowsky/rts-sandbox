using UnityEngine;
using Assets.Scripts.Infrastructure.Events;
using System.Collections.Generic;
using System;
using System.Linq;
using Assets.Scripts.Infrastructure.Abstractions;
using Assets.Scripts.Infrastructure.Enums;
using Assets.SkillsSection.Scripts.Events;
using Assets.SkillsSection.Scripts;
using static Assets.SkillsSection.Scripts.UnitSkills;

namespace Assets.Scripts.GameObjects
{
    public class UnitCommandManager : MonoBehaviour
    {
        public List<string> CommandListInfo;
        public string CurrentRunningCommandInfo;

        public bool HasCommandInQueue { get => CommandsQueue.Any(); }

        private UnitEventManager _unitEventManager;

        private ICommand CurrentRunningCommand;
        private Queue<ICommand> CommandsQueue = new Queue<ICommand>();
        private PlayerEventController _playerEventController;
        private UnitSkills _unitSkills;
        private UnitBehaviour.UnitBehaviourManager _behaviourManager;

        /// <summary>
        /// Stunned units still take orders — the order lands in the queue and runs
        /// the moment the stun is over (M-019, decision of the user). So the queue
        /// is not closed here, only held: nothing is started while this is up.
        /// </summary>
        private bool _isStunned;

        void Awake()
        {
            _unitEventManager = GetComponent<UnitEventManager>();
            _unitSkills = GetComponent<UnitSkills>();
            _behaviourManager = GetComponent<UnitBehaviour.UnitBehaviourManager>();
            _playerEventController = GameObject.FindGameObjectWithTag(Tag.PlayerController.ToString())
                .GetComponent<PlayerEventController>();

            _unitEventManager.MoveCommandReceived += StartMoveCommand;
            _unitEventManager.AttackCommandReceived += StartAttackCommand;
            _unitEventManager.FollowCommandReceived += StartFollowCommand;
            _unitEventManager.AMoveCommandReceived += StartAMoveCommand;
            _unitEventManager.HoldCommandReceived += StartHoldCommand;
            _unitEventManager.BuildCommandReceived += StartBuildCommand;
            _unitEventManager.MineCommandReceived += StartMineCommand;
            _unitEventManager.HarvestingCommandReceived += StartHarvestingCommand;
            _unitEventManager.SkillCastCommandReceived += StartSkillCastCommand;

            _unitEventManager.MoveActionEnded += RunNextCommand;
            _unitEventManager.AttackActionEnded += RunNextCommand;
            _unitEventManager.FollowActionEnded += RunNextCommand;
            _unitEventManager.AMoveActionEnded += RunNextCommand;
            _unitEventManager.BuildActionEnded += RunNextCommand;
            _unitEventManager.MineActionEnded += RunNextCommand;
            _unitEventManager.HarvestingActionEnded += RunNextCommand;
            _unitEventManager.SkillCastActionEnded += RunNextCommand;

            _unitEventManager.StunStarted += OnStunStarted;
            _unitEventManager.StunEnded += OnStunEnded;
        }

        private void Start()
        {
            if (CurrentRunningCommand == null)
            {
                SetIdleState();
            }
        }

        protected void StartMoveCommand(MoveCommandReceivedEventArgs args)
        {
            var moveCommand = new MoveCommand(_unitEventManager, args);

            StartCommand(moveCommand, args.AddToCommandsQueue);
        }

        protected void StartAttackCommand(AttackCommandReceivedEventArgs args)
        {
            var attackCommand = new AttackCommand(_unitEventManager, args);

            StartCommand(attackCommand, args.AddToCommandsQueue);
        }

        protected void StartFollowCommand(FollowCommandReceivedEventArgs args)
        {
            var followCommand = new FollowCommand(_unitEventManager, args);

            StartCommand(followCommand, args.AddToCommandsQueue);
        }

        protected void StartAMoveCommand(MoveCommandReceivedEventArgs args)
        {
            var moveCommand = new AMoveCommand(_unitEventManager, args);

            StartCommand(moveCommand, args.AddToCommandsQueue);
        }

        protected void StartHoldCommand(HoldCommandReceivedEventArgs args)
        {
            var holdCommand = new HoldCommand(_unitEventManager, args);

            StartCommand(holdCommand, args.AddToCommandsQueue);
        }

        protected void StartBuildCommand(BuildCommandReceivedEventArgs args)
        {
            var buildCommand = new BuildCommand(_unitEventManager, args);

            StartCommand(buildCommand, args.AddToCommandsQueue);
        }

        protected void StartMineCommand(MineCommandReceivedEventArgs args)
        {
            var mineCommand = new MineCommand(_unitEventManager, args);

            StartCommand(mineCommand, args.AddToCommandsQueue);
        }

        protected void StartHarvestingCommand(HarvestingCommandReceivedEventArgs args)
        {
            var harvestingCommand = new HarvestingCommand(_unitEventManager, args);

            StartCommand(harvestingCommand, args.AddToCommandsQueue);
        }

        protected void StartSkillCastCommand(SkillCastCommandReceivedEventArgs args)
        {
            var skillCastCommand = new SkillCastCommand(_unitEventManager, _unitSkills, _behaviourManager, args);

            StartCommand(skillCastCommand, args.AddToCommandsQueue);
        }

        protected void RunNextCommand(EventArgs args)
        {
            // Held, not closed: whatever is in the queue stays there and the unit
            // picks it up when the stun is over. Every ActionStarted of the game
            // goes out from this class, so this one gate is enough to keep a
            // stunned unit from doing anything.
            if (_isStunned)
            {
                return;
            }

            if (CommandsQueue.Count > 0)
            {
                CommandListInfo.RemoveAt(0);
                TriggerEventCurrentCommandEnded();
                CurrentRunningCommand = CommandsQueue.Dequeue();
                CurrentRunningCommandInfo = CurrentRunningCommand.GetType().Name;
                if (CurrentRunningCommand.Check())
                {
                    CurrentRunningCommand.Start();
                }
                else
                {
                    RunNextCommand(args);
                }
            }
            else
            {
                SetIdleState();
            }
        }

        private void OnStunStarted(StunStartedEventArgs args)
        {
            _isStunned = true;
        }

        /// <summary>
        /// Out of the stun: the unit goes on with what it was told. The command it
        /// was running is started over — a walk simply continues, a cast that the
        /// stun broke is cast again from the beginning, because the order is still
        /// the order (M-019).
        /// </summary>
        private void OnStunEnded(StunEndedEventArgs args)
        {
            _isStunned = false;

            if (CurrentRunningCommand != null && CurrentRunningCommand.Check())
            {
                CurrentRunningCommand.Start();
                return;
            }

            // Nothing to go back to, or it stopped making sense while the unit
            // stood there: take the next one, or go idle.
            CurrentRunningCommand = null;
            RunNextCommand(new EventArgs());
        }

        private void SetIdleState()
        {
            TriggerEventCurrentCommandEnded();
            CurrentRunningCommand = null;
            CurrentRunningCommandInfo = "Idle";
            _unitEventManager.OnAutoAttackIdleStarted(gameObject.transform.position);
        }

        private void StartCommand(ICommand command , bool addToCommandsQueue)
        {
            if (!addToCommandsQueue)
            {
                TriggerEventCommandsQueueCleared();
                CommandsQueue.Clear();
                CommandListInfo.Clear();
                TriggerEventCurrentCommandEnded();
                CurrentRunningCommand = null;
                CurrentRunningCommandInfo = "";
            }

            var commandName = command.GetType().Name;
            CommandListInfo.Add(commandName);

            TriggerEventCommandAddedToQueue(command);
            CommandsQueue.Enqueue(command);

            if (CurrentRunningCommand == null)
            {
                RunNextCommand(new EventArgs());
            }
        }

        private void TriggerEventCurrentCommandEnded()
        {
            _playerEventController.OnCurrentCommandEnded(CurrentRunningCommand);
        }

        private void TriggerEventCommandAddedToQueue(ICommand command)
        {
            _playerEventController.OnCommandAddedToQueue(command);
        }

        private void TriggerEventCommandsQueueCleared()
        {
            _playerEventController.OnCommandsQueueCleared(CommandsQueue);
        }

        #region Commands
        private class MoveCommand : ICommand
        {
            public MoveCommandReceivedEventArgs args;

            private UnitEventManager _unitEventManager;

            public MoveCommand(UnitEventManager unitEventManager, MoveCommandReceivedEventArgs args)
            {
                this.args = args;
                _unitEventManager = unitEventManager;
            }

            public bool Check()
            {
                return args.MovePoint != null;
            }

            public void Start()
            {
                _unitEventManager.OnMoveActionStarted(args.MovePoint);
            }
        }

        private class AttackCommand : ICommand
        {
            public AttackCommandReceivedEventArgs args;

            private UnitEventManager _unitEventManager;

            public AttackCommand(UnitEventManager unitEventManager, AttackCommandReceivedEventArgs args)
            {
                this.args = args;
                _unitEventManager = unitEventManager;
            }

            public bool Check()
            {
                return args.Target != null;
            }

            public void Start()
            {
                _unitEventManager.OnAttackActionStarted(args.Target);
            }
        }

        private class FollowCommand : ICommand
        {
            public FollowCommandReceivedEventArgs args;

            private UnitEventManager _unitEventManager;

            public FollowCommand(UnitEventManager unitEventManager, FollowCommandReceivedEventArgs args)
            {
                this.args = args;
                _unitEventManager = unitEventManager;
            }

            public bool Check()
            {
                return args.Target != null;
            }

            public void Start()
            {
                _unitEventManager.OnFollowActionStarted(args.Target);
            }
        }

        private class AMoveCommand : ICommand
        {
            public MoveCommandReceivedEventArgs args;

            private UnitEventManager _unitEventManager;

            public AMoveCommand(UnitEventManager unitEventManager, MoveCommandReceivedEventArgs args)
            {
                this.args = args;
                _unitEventManager = unitEventManager;
            }

            public bool Check()
            {
                return args.MovePoint != null;
            }

            public void Start()
            {
                _unitEventManager.OnAMoveActionStarted(args.MovePoint);
            }
        }

        private class BuildCommand : IBuildCommand
        {
            public BuildCommandReceivedEventArgs args;

            private UnitEventManager _unitEventManager;

            public BuildCommand(UnitEventManager unitEventManager, BuildCommandReceivedEventArgs args)
            {
                this.args = args;
                _unitEventManager = unitEventManager;
            }

            public bool Check()
            {
                return args.Point != null && args.Building != null;
            }

            public void Start()
            {
                _unitEventManager.OnBuildActionStarted(args.Point, args.Building, args.IsMineHeld, args.MineToHeld);
            }

            public GameObject GetBuildingObject()
            {
                return args.Building;
            }

            public Vector3 GetPoint()
            {
                return args.Point;
            }
        }

        private class HoldCommand : ICommand
        {
            public HoldCommandReceivedEventArgs args;

            private UnitEventManager _unitEventManager;

            public HoldCommand(UnitEventManager unitEventManager, HoldCommandReceivedEventArgs args)
            {
                this.args = args;
                _unitEventManager = unitEventManager;
            }

            public bool Check()
            {
                return true;
            }

            public void Start()
            {
                _unitEventManager.OnHoldActionStarted();
            }
        }

        private class MineCommand : ICommand
        {
            public MineCommandReceivedEventArgs args;

            private UnitEventManager _unitEventManager;

            public MineCommand(UnitEventManager unitEventManager, MineCommandReceivedEventArgs args)
            {
                this.args = args;
                _unitEventManager = unitEventManager;
            }

            public bool Check()
            {
                return args.Mine != null;
            }

            public void Start()
            {
                _unitEventManager.OnMineActionStarted(args.Mine);
            }
        }

        private class HarvestingCommand : ICommand
        {
            public HarvestingCommandReceivedEventArgs args;

            private UnitEventManager _unitEventManager;

            public HarvestingCommand(UnitEventManager unitEventManager, HarvestingCommandReceivedEventArgs args)
            {
                this.args = args;
                _unitEventManager = unitEventManager;
            }

            public bool Check()
            {
                return true;
            }

            public void Start()
            {
                _unitEventManager.OnHarvestingActionStarted(args.Resource, args.Storage, args.ToStorage);
            }
        }

        private class SkillCastCommand : ICommand
        {
            public SkillCastCommandReceivedEventArgs args;

            private UnitEventManager _unitEventManager;

            private UnitSkills _unitSkills;

            private UnitBehaviour.UnitBehaviourManager _behaviourManager;

            public SkillCastCommand(UnitEventManager unitEventManager, UnitSkills unitSkills,
                UnitBehaviour.UnitBehaviourManager behaviourManager, SkillCastCommandReceivedEventArgs args)
            {
                this.args = args;
                _unitEventManager = unitEventManager;
                _unitSkills = unitSkills;
                _behaviourManager = behaviourManager;
            }

            public bool Check()
            {
                if (_unitSkills == null)
                {
                    return false;
                }

                if (!_unitSkills.CheckIfCanCast(args.UnitSkill))
                {
                    return false;
                }

                // Nobody on this unit can take this cast — drop the command the
                // same way a dead target or missing mana drops one. Started, it
                // would never send ActionEnded and the queue would hang forever.
                return _behaviourManager != null
                    && _behaviourManager.CanHandle(UnitBehaviour.UnitActionType.SkillCast, args.ToActionArgs());
            }

            public void Start()
            {
                var actionArgs = args.ToActionArgs();
                _unitEventManager.OnSkillCastActionStarted(actionArgs);
            }
        }

        #endregion

        private void OnDestroy()
        {
            // Start may never have run: an object destroyed in the frame it
            // appeared, or one never activated, reaches OnDestroy with this
            // still null.
            if (_unitEventManager == null)
            {
                return;
            }

            _unitEventManager.MoveCommandReceived -= StartMoveCommand;
            _unitEventManager.AttackCommandReceived -= StartAttackCommand;
            _unitEventManager.FollowCommandReceived -= StartFollowCommand;
            _unitEventManager.AMoveCommandReceived -= StartAMoveCommand;
            _unitEventManager.HoldCommandReceived -= StartHoldCommand;
            _unitEventManager.BuildCommandReceived -= StartBuildCommand;
            _unitEventManager.MineCommandReceived -= StartMineCommand;
            _unitEventManager.HarvestingCommandReceived -= StartHarvestingCommand;
            _unitEventManager.SkillCastCommandReceived -= StartSkillCastCommand;

            _unitEventManager.MoveActionEnded -= RunNextCommand;
            _unitEventManager.AttackActionEnded -= RunNextCommand;
            _unitEventManager.FollowActionEnded -= RunNextCommand;
            _unitEventManager.AMoveActionEnded -= RunNextCommand;
            _unitEventManager.BuildActionEnded -= RunNextCommand;
            _unitEventManager.MineActionEnded -= RunNextCommand;
            _unitEventManager.HarvestingActionEnded -= RunNextCommand;
            _unitEventManager.SkillCastActionEnded -= RunNextCommand;

            _unitEventManager.StunStarted -= OnStunStarted;
            _unitEventManager.StunEnded -= OnStunEnded;
        }
    }
}
