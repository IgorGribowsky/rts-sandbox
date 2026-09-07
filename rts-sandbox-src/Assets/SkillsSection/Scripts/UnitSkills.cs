using Assets.Scripts;
using Assets.Scripts.Infrastructure.Constants;
using Assets.Scripts.Infrastructure.Events;
using Assets.SkillsSection.Scripts.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.SkillsSection.Scripts
{
    public class UnitSkills : MonoBehaviour
    {
        /// <summary>What this unit is able to cast. Setup only, no combat state.</summary>
        public List<SkillSlot> Skills;

        private PlayerEventController _playerEventController;

        private UnitValues _unitValues;

        private UnitEventManager _unitEventManager;

        private CursorMovedEventArgs _cursorMovedEventArgs;

        /// <summary>
        /// Cooldowns, passive executors and everything else that only makes sense
        /// while playing. Holds passives too, though those are never castable.
        /// </summary>
        private readonly List<UnitSkill> _runtimeSkills = new List<UnitSkill>();

        void Awake()
        {
            BuildRuntimeSkills();
        }

        void Start()
        {
            _playerEventController = GameServices.PlayerEventController;
            _unitValues = GetComponent<UnitValues>();
            _unitEventManager = GetComponent<UnitEventManager>();
            _playerEventController.CursorMoved += CursorMovedHandler;

            // Not in Awake: a passive may reach for other components of the unit,
            // and by Start they have all woken up.
            ActivatePassives();
        }

        void Update()
        {
            foreach (var skill in _runtimeSkills)
            {
                skill.Tick(Time.deltaTime);
            }
        }

        /// <summary>
        /// Arguments of the cast order for this skill, or null when there is
        /// nothing to cast at — a cast aimed at a unit released over the wrong one.
        /// </summary>
        public SkillCastCommandReceivedEventArgs CreateCommandArgs(UnitSkill unitSkill, bool addToCommandsQueue = false)
        {
            if (!_runtimeSkills.Contains(unitSkill))
            {
                throw new ArgumentException();
            }

            SkillCastCommandReceivedEventArgs args;

            var activeSkill = unitSkill.Skill as ActiveSkill;

            if (activeSkill.Action is CastToTargetAction targetAction)
            {
                var target = _cursorMovedEventArgs?.UnitUnderCursor;

                // The key was released not over a suitable target: the cast is
                // cancelled outright — no order, no mana, no cooldown (decision
                // of the user, answer in chat 2026-09-07). null means "there is
                // nothing to give an order about".
                if (target == null || !SkillTargetFilter.CanHit(target, gameObject, targetAction.TargetType))
                {
                    return null;
                }

                return new SkillCastToTargetCommandReceivedEventArgs(unitSkill, target, addToCommandsQueue);
            }

            if (activeSkill.Action is CastToPointAction)
            {
                args = new SkillCastToPointCommandReceivedEventArgs(unitSkill, _cursorMovedEventArgs.CursorPosition, addToCommandsQueue);
            }
            else
            {
                args = new SkillCastCommandReceivedEventArgs(unitSkill, addToCommandsQueue);
            }

            return args;
        }

        public void Cast(UnitSkill unitSkill, SkillParams skillParams)
        {
            var activeSkill = unitSkill.Skill as ActiveSkill;
            SkillActionExecutorFactory.Create(activeSkill.Action)?.Act(skillParams);
            unitSkill.StartCooldown(activeSkill.Cooldown);
            var spendMana = activeSkill.ManaCost;
            _unitEventManager.OnManaUsed(spendMana);
        }

        public bool CheckIfCanCast(UnitSkill unitSkill)
        {
            if (unitSkill == null) return false;
            if (!_runtimeSkills.Contains(unitSkill)) return false;
            if (unitSkill.CurrentCooldown > 0) return false;
            if (unitSkill.Skill is not ActiveSkill) return false;
            if (_unitValues.CurrentMana < (unitSkill.Skill as ActiveSkill).ManaCost) return false;

            return true;
        }

        public UnitSkill GetSkillByKeycode(KeyCode keycode)
        {
            // None is "no key at all", not a key: a passive sits on it, and
            // asking for None must not hand a passive back to the caster.
            if (keycode == KeyCode.None)
            {
                return null;
            }

            return _runtimeSkills.FirstOrDefault(x => x.Keycode == keycode);
        }

        private void BuildRuntimeSkills()
        {
            _runtimeSkills.Clear();

            if (Skills == null)
            {
                return;
            }

            foreach (var slot in Skills)
            {
                if (slot == null || slot.Skill == null)
                {
                    continue;
                }

                _runtimeSkills.Add(new UnitSkill(slot));
            }
        }

        /// <summary>
        /// Turns on every passive of the unit. A passive cannot be cast by a key,
        /// so this is the only moment it ever starts (M-015).
        /// </summary>
        private void ActivatePassives()
        {
            foreach (var unitSkill in _runtimeSkills)
            {
                if (unitSkill.Skill is not PassiveSkill passiveSkill)
                {
                    continue;
                }

                var executor = PassiveSkillExecutorFactory.Create(passiveSkill.Action);

                if (executor == null)
                {
                    continue;
                }

                unitSkill.AttachPassive(executor);
                executor.Activate(gameObject);
            }
        }

        /// <summary>Everything a passive subscribed to is released here.</summary>
        private void DeactivatePassives()
        {
            foreach (var unitSkill in _runtimeSkills)
            {
                unitSkill.DetachPassive(gameObject);
            }
        }

        private void OnValidate()
        {
            if (Skills == null)
            {
                return;
            }

            foreach (var slot in Skills)
            {
                if (slot == null) continue;

                if (slot.IsBanned())
                    slot.Keycode = KeyCode.None;

                // A passive cannot be cast by a key. Left on one it would eat the
                // press silently: CheckIfCanCast turns it down and SkillController
                // then finds nobody to cast with.
                if (slot.Skill is PassiveSkill)
                    slot.Keycode = KeyCode.None;
            }
        }

        protected void CursorMovedHandler(CursorMovedEventArgs args)
        {
            _cursorMovedEventArgs = args;
        }

        /// <summary>
        /// One line of the setup on the prefab: which skill sits on which key.
        /// Serialized, so its field names must not change lightly.
        /// </summary>
        [Serializable]
        public class SkillSlot
        {
            public Skill Skill;

            public KeyCode Keycode;

            /// <summary>The game itself took this key, see ReservedKeys.</summary>
            public bool IsBanned() => ReservedKeys.IsReserved(Keycode);
        }

        /// <summary>
        /// A skill of one particular unit while the game runs: the setup it came
        /// from plus the state of casting it. Never serialized — it is built in
        /// Awake and dies with the unit.
        /// </summary>
        public class UnitSkill
        {
            public UnitSkill(SkillSlot slot)
            {
                Slot = slot;
            }

            public SkillSlot Slot { get; }

            public Skill Skill => Slot.Skill;

            public KeyCode Keycode => Slot.Keycode;

            public float CurrentCooldown { get; private set; }

            /// <summary>
            /// Set for a passive only: it works the whole time, so its executor
            /// lives here, in the unit's own state, and dies with the unit.
            /// </summary>
            public PassiveSkillExecutor Passive { get; private set; }

            internal void StartCooldown(float cd)
            {
                CurrentCooldown = cd;
            }

            internal void AttachPassive(PassiveSkillExecutor executor)
            {
                Passive = executor;
            }

            internal void DetachPassive(GameObject owner)
            {
                if (Passive == null)
                {
                    return;
                }

                Passive.Deactivate(owner);
                Passive = null;
            }

            public void Tick(float deltaTime)
            {
                if (CurrentCooldown > 0)
                    CurrentCooldown = Mathf.Max(0, CurrentCooldown - deltaTime);
            }
        }

        private void OnDestroy()
        {
            if (_playerEventController != null)
            {
                _playerEventController.CursorMoved -= CursorMovedHandler;
            }

            DeactivatePassives();
        }
    }
}
