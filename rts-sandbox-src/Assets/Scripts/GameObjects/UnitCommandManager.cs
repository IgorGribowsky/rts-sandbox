using UnityEngine;
using Assets.Scripts.Infrastructure.Events;
using System.Collections.Generic;
using System;

namespace Assets.Scripts.GameObjects
{
    public class UnitCommandManager : MonoBehaviour
    {
        public List<string> CommandListInfo;
        public string CurrentRunningCommandInfo;

        private UnitEventManager _unitEventManager;

        private ICommand CurrentRunningCommand;
        private Queue<ICommand> CommandsQueue = new Queue<ICommand>();

        void Awake()
        {
            _unitEventManager = GetComponent<UnitEventManager>();

            _unitEventManager.MoveCommandReceived += StartMoveCommand;
            _unitEventManager.AttackCommandReceived += StartAttackCommand;
            _unitEventManager.FollowCommandReceived += StartFollowCommand;
            _unitEventManager.AMoveCommandReceived += StartAMoveCommand;

            _unitEventManager.MoveActionEnded += RunNextCommand;
            _unitEventManager.AttackActionEnded += RunNextCommand;
            _unitEventManager.FollowActionEnded += RunNextCommand;
            _unitEventManager.AMoveActionEnded += RunNextCommand;

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

        protected void RunNextCommand(EventArgs args)
        {
            if (CommandsQueue.Count > 0)
            {
                CommandListInfo.RemoveAt(0);
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
                CurrentRunningCommand = null;
                CurrentRunningCommandInfo = "";
            }

        }

        private void StartCommand(ICommand command , bool addToCommandsQueue)
        {
            if (!addToCommandsQueue)
            {
                CommandsQueue.Clear();
                CommandListInfo.Clear();
                CurrentRunningCommand = null;
                CurrentRunningCommandInfo = "";
            }

            var commandName = command.GetType().Name;
            CommandListInfo.Add(commandName);

            CommandsQueue.Enqueue(command);

            if (CurrentRunningCommand == null)
            {
                RunNextCommand(new EventArgs());
            }
        }

        #region Commands
        private interface ICommand
        {
            bool Check();
            void Start();
        }

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

        #endregion
    }
}
