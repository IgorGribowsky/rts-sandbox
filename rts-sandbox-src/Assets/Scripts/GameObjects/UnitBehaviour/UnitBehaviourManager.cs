using Assets.Scripts.Infrastructure.Events;
using Assets.SkillsSection.Scripts.Events;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.GameObjects.UnitBehaviour
{
    /// <summary>
    /// Holds the behaviours a unit is able to run and keeps exactly one of them
    /// current. What the unit can do is the Behaviours list on the prefab.
    /// </summary>
    public class UnitBehaviourManager : MonoBehaviour
    {
        [SerializeField]
        private List<UnitBehaviourType> Behaviours = new List<UnitBehaviourType>();

        /// <summary>Shown in the inspector while playing, same idea as UnitCommandManager.</summary>
        public string CurrentBehaviourInfo;

        private UnitEventManager _unitEventManager;

        private readonly List<UnitBehaviourBase> _all = new List<UnitBehaviourBase>();
        private readonly Dictionary<UnitActionType, List<UnitBehaviourBase>> _byAction
            = new Dictionary<UnitActionType, List<UnitBehaviourBase>>();
        private readonly Dictionary<Type, UnitBehaviourBase> _byClass
            = new Dictionary<Type, UnitBehaviourBase>();

        private UnitBehaviourBase _current;

        public void Awake()
        {
            _unitEventManager = GetComponent<UnitEventManager>();

            CreateBehaviours();

            _unitEventManager.MoveActionStarted += StartMove;
            _unitEventManager.AMoveActionStarted += StartAMove;
            _unitEventManager.FollowActionStarted += StartFollow;
            _unitEventManager.HoldActionStarted += StartHold;
            _unitEventManager.AttackActionStarted += StartAttack;
            _unitEventManager.AutoAttackIdleStarted += StartAutoAttackIdle;
            _unitEventManager.BuildActionStarted += StartBuild;
            _unitEventManager.MineActionStarted += StartMine;
            _unitEventManager.HarvestingActionStarted += StartHarvest;
            _unitEventManager.SkillCastActionStarted += StartSkillCast;
            _unitEventManager.StunStarted += StartStun;
            _unitEventManager.StunEnded += EndStun;
        }

        public void Update()
        {
            for (var i = 0; i < _all.Count; i++)
            {
                _all[i].Tick();
            }
        }

        /// <summary>The behaviour of this exact class, or null if the unit has none.</summary>
        public T Get<T>() where T : UnitBehaviourBase
        {
            _byClass.TryGetValue(typeof(T), out var behaviour);
            return behaviour as T;
        }

        public bool Has<T>() where T : UnitBehaviourBase => Get<T>() != null;

        public bool IsBehaviourActive<T>() where T : UnitBehaviourBase => Get<T>()?.IsActive ?? false;

        /// <summary>
        /// Is there a behaviour on this unit able to take this order? Asked by
        /// UnitCommandManager before it starts a command: an order nobody can take
        /// never sends ActionEnded, and the queue would wait for it forever.
        /// </summary>
        public bool CanHandle(UnitActionType action, EventArgs args) => Select(action, args) != null;

        /// <summary>Used by auto attack to reach the attack behaviour of its own unit.</summary>
        public UnitBehaviourBase GetForAction(UnitActionType action)
        {
            if (_byAction.TryGetValue(action, out var candidates) && candidates.Count > 0)
            {
                return candidates[0];
            }

            return null;
        }

        private void CreateBehaviours()
        {
            var context = new UnitBehaviourContext(gameObject, this);

            foreach (var type in Behaviours)
            {
                var behaviour = UnitBehaviourFactory.Create(type);

                if (behaviour == null)
                {
                    Debug.LogError("No behaviour class for " + type + " on " + name + ".", this);
                    continue;
                }

                _all.Add(behaviour);
                _byClass[behaviour.GetType()] = behaviour;

                if (!_byAction.TryGetValue(behaviour.Trigger, out var candidates))
                {
                    candidates = new List<UnitBehaviourBase>();
                    _byAction[behaviour.Trigger] = candidates;
                }

                candidates.Add(behaviour);
            }

            // Second pass: a behaviour may look its neighbours up on initialization.
            foreach (var behaviour in _all)
            {
                behaviour.Initialize(context);
            }
        }

        private void Activate(UnitActionType action, EventArgs args)
        {
            var next = Select(action, args);

            if (next == null)
            {
                return;
            }

            if (_current != null && _current != next)
            {
                _current.Deactivate();
            }

            _current = next;
            CurrentBehaviourInfo = next.GetType().Name;
            _current.Activate(args);
        }

        private UnitBehaviourBase Select(UnitActionType action, EventArgs args)
        {
            if (!_byAction.TryGetValue(action, out var candidates))
            {
                return null;
            }

            foreach (var candidate in candidates)
            {
                if (candidate.CanHandle(args))
                {
                    return candidate;
                }
            }

            return null;
        }

        private void StartMove(MoveActionStartedEventArgs args) => Activate(UnitActionType.Move, args);

        private void StartAMove(MoveActionStartedEventArgs args) => Activate(UnitActionType.AMove, args);

        private void StartFollow(FollowActionStartedEventArgs args) => Activate(UnitActionType.Follow, args);

        private void StartHold(HoldActionStartedEventArgs args) => Activate(UnitActionType.Hold, args);

        private void StartAttack(AttackActionStartedEventArgs args) => Activate(UnitActionType.Attack, args);

        private void StartAutoAttackIdle(AutoAttackIdleStartedEventArgs args) => Activate(UnitActionType.AutoAttackIdle, args);

        private void StartBuild(BuildActionStartedEventArgs args) => Activate(UnitActionType.Build, args);

        private void StartMine(MineActionStartedEventArgs args) => Activate(UnitActionType.Mine, args);

        private void StartHarvest(HarvestingActionStartedEventArgs args) => Activate(UnitActionType.Harvest, args);

        private void StartSkillCast(SkillCastActionStartedEventArgs args) => Activate(UnitActionType.SkillCast, args);

        /// <summary>
        /// A stun landed. It goes through the ordinary Activate, so it smothers
        /// whatever was running the way any behaviour does — that is what breaks a
        /// cast and stops a walk (M-019).
        /// </summary>
        private void StartStun(StunStartedEventArgs args)
        {
            if (Select(UnitActionType.Stun, args) == null)
            {
                // Loud on purpose: without the behaviour the unit would keep acting
                // while its command queue is held shut by the stun, and that would
                // look like a frozen unit with no reason on screen.
                Debug.LogError("Stunned while " + name + " has no Stunned behaviour in its list.", this);
                return;
            }

            Activate(UnitActionType.Stun, args);
        }

        /// <summary>
        /// The stun is over. Only the behaviour is dropped here; what the unit does
        /// next is the business of the command queue, which resumes on the same
        /// event (M-004).
        /// </summary>
        private void EndStun(StunEndedEventArgs args)
        {
            if (_current is not StunnedBehaviour)
            {
                return;
            }

            _current.Deactivate();
            _current = null;
            CurrentBehaviourInfo = string.Empty;
        }

        private void OnDestroy()
        {
            _unitEventManager.MoveActionStarted -= StartMove;
            _unitEventManager.AMoveActionStarted -= StartAMove;
            _unitEventManager.FollowActionStarted -= StartFollow;
            _unitEventManager.HoldActionStarted -= StartHold;
            _unitEventManager.AttackActionStarted -= StartAttack;
            _unitEventManager.AutoAttackIdleStarted -= StartAutoAttackIdle;
            _unitEventManager.BuildActionStarted -= StartBuild;
            _unitEventManager.MineActionStarted -= StartMine;
            _unitEventManager.HarvestingActionStarted -= StartHarvest;
            _unitEventManager.SkillCastActionStarted -= StartSkillCast;
            _unitEventManager.StunStarted -= StartStun;
            _unitEventManager.StunEnded -= EndStun;

            foreach (var behaviour in _all)
            {
                behaviour.Dispose();
            }
        }
    }
}
