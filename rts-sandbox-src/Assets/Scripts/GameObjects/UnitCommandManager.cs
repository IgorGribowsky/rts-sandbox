using UnityEngine.AI;
using UnityEngine;
using Assets.Scripts.Infrastructure.Events;
using NUnit.Framework;
using System.Collections.Generic;
using System;
using Unity.VisualScripting;
using Assets.Scripts.Infrastructure.Events.Common;

namespace Assets.Scripts.GameObjects
{
    public class UnitCommandManager : MonoBehaviour
    {
        private UnitEventManager _unitEventManager;

        private ICommand CurrentRunningCommand;
        private Queue<ICommand> CommandsQueue = new Queue<ICommand>();

        void Start()
        {
            _unitEventManager = GetComponent<UnitEventManager>();

            _unitEventManager.MoveCommandReceived += StartMoveCommand;
            _unitEventManager.AttackCommandReceived += StartAttackCommand;
            _unitEventManager.FollowCommandReceived += StartFollowCommand;

            _unitEventManager.MoveActionEnded += RunNextCommand;
            _unitEventManager.AttackActionEnded += RunNextCommand;
            _unitEventManager.FollowActionEnded += RunNextCommand;

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

        protected void RunNextCommand(EventArgs args)
        {
            if (CommandsQueue.Count > 0)
            {
                CurrentRunningCommand = CommandsQueue.Dequeue();
                CurrentRunningCommand.Start();
            }
            else
            {
                CurrentRunningCommand = null;
            }

        }

        private void StartCommand(ICommand command , bool addToCommandsQueue)
        {
            if (!addToCommandsQueue)
            {
                CommandsQueue.Clear();
                CurrentRunningCommand = null;
            }

            CommandsQueue.Enqueue(command);

            if (CurrentRunningCommand == null)
            {
                RunNextCommand(new EventArgs());
            }
        }

        #region Commands
        private interface ICommand
        {
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

            public void Start()
            {
                _unitEventManager.OnFollowActionStarted(args.Target);
            }
        }

        #endregion
    }
}
