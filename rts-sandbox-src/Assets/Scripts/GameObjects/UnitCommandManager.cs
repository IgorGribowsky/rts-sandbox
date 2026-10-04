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
        public List<string> CommandListInfo = new List<string>();
        public string CurrentRunningCommandInfo;

        public bool HasCommandInQueue { get => CommandsQueue.Any(); }

        /// <summary>The order running now; Idle when the queue is empty (M-023).</summary>
        public UnitCommandKind CurrentCommandKind => CurrentRunningCommand?.Kind ?? UnitCommandKind.Idle;

        /// <summary>The skill being cast or walked up to, null for any other order.</summary>
        public UnitSkill CurrentSkill => (CurrentRunningCommand as SkillCastCommand)?.args.UnitSkill;

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

        }

        private void OnEnable()
        {
            _unitEventManager.MoveCommandReceived += StartMoveCommand;
            _unitEventManager.AttackCommandReceived += StartAttackCommand;
            _unitEventManager.FollowCommandReceived += StartFollowCommand;
            _unitEventManager.AMoveCommandReceived += StartAMoveCommand;
            _unitEventManager.HoldCommandReceived += StartHoldCommand;
            _unitEventManager.BuildCommandReceived += StartBuildCommand;
            _unitEventManager.MineCommandReceived += StartMineCommand;
            _unitEventManager.HarvestingCommandReceived += StartHarvestingCommand;
            _unitEventManager.GatherCommandReceived += StartGatherCommand;
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

        private void OnDisable()
        {
            _unitEventManager.MoveCommandReceived -= StartMoveCommand;
            _unitEventManager.AttackCommandReceived -= StartAttackCommand;
            _unitEventManager.FollowCommandReceived -= StartFollowCommand;
            _unitEventManager.AMoveCommandReceived -= StartAMoveCommand;
            _unitEventManager.HoldCommandReceived -= StartHoldCommand;
            _unitEventManager.BuildCommandReceived -= StartBuildCommand;
            _unitEventManager.MineCommandReceived -= StartMineCommand;
            _unitEventManager.HarvestingCommandReceived -= StartHarvestingCommand;
            _unitEventManager.GatherCommandReceived -= StartGatherCommand;
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

        protected void StartGatherCommand(GatherCommandReceivedEventArgs args)
        {
            StartCommand(new GatherCommand(_unitEventManager, gameObject), args.AddToCommandsQueue);
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
                SetCurrentCommand(CommandsQueue.Dequeue());
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
            SetCurrentCommand(null);
            RunNextCommand(new EventArgs());
        }

        private void SetIdleState()
        {
            TriggerEventCurrentCommandEnded();
            SetCurrentCommand(null);
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
                SetCurrentCommand(null);
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

        /// <summary>
        /// The one place the running order changes, so that whoever shows it
        /// hears about every change (M-023).
        /// </summary>
        private void SetCurrentCommand(ICommand command)
        {
            if (CurrentRunningCommand == command)
            {
                return;
            }

            CurrentRunningCommand = command;
            _unitEventManager.OnCurrentCommandChanged(command);
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
            public UnitCommandKind Kind => UnitCommandKind.Move;

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
            public UnitCommandKind Kind => UnitCommandKind.Attack;

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
            public UnitCommandKind Kind => UnitCommandKind.Follow;

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
            public UnitCommandKind Kind => UnitCommandKind.AMove;

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
            public UnitCommandKind Kind => UnitCommandKind.Build;

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

            public UnitTypeData GetBuildingType()
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
            public UnitCommandKind Kind => UnitCommandKind.Hold;

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
            public UnitCommandKind Kind => UnitCommandKind.Mine;

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
            public UnitCommandKind Kind => UnitCommandKind.Harvest;

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

        /// <summary>
        /// "Gather" (M-023): the target is chosen when the order starts, not when
        /// it was given, so a queued order goes to what is nearest by then. Runs
        /// as an ordinary harvesting or mining action from there on.
        /// </summary>
        private class GatherCommand : ICommand
        {
            public UnitCommandKind Kind => UnitCommandKind.Gather;

            private readonly UnitEventManager _unitEventManager;
            private readonly GameObject _unit;

            public GatherCommand(UnitEventManager unitEventManager, GameObject unit)
            {
                _unitEventManager = unitEventManager;
                _unit = unit;
            }

            public bool Check()
            {
                return GatherTargets.TryFind(_unit, out _);
            }

            public void Start()
            {
                if (!GatherTargets.TryFind(_unit, out var target))
                {
                    // Gone between Check and Start: end at once so the queue moves on.
                    _unitEventManager.OnHarvestingActionEnded();
                    return;
                }

                switch (target.Kind)
                {
                    case GatherTargetKind.Deliver:
                        _unitEventManager.OnHarvestingActionStarted(null, target.Target, true);
                        break;
                    case GatherTargetKind.Harvest:
                        _unitEventManager.OnHarvestingActionStarted(target.Target, null, false);
                        break;
                    case GatherTargetKind.Mine:
                        _unitEventManager.OnMineActionStarted(target.Target);
                        break;
                }
            }
        }

        private class SkillCastCommand : ICommand
        {
            public UnitCommandKind Kind => UnitCommandKind.SkillCast;

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
    }
}
