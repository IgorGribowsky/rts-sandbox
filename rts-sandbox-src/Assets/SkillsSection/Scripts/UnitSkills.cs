using Assets.Scripts;
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

        /// <summary>Cooldowns and everything else that only makes sense while playing.</summary>
        private readonly List<UnitSkill> _castable = new List<UnitSkill>();

        void Awake()
        {
            BuildCastableSkills();
        }

        void Start()
        {
            _playerEventController = GameServices.PlayerEventController;
            _unitValues = GetComponent<UnitValues>();
            _unitEventManager = GetComponent<UnitEventManager>();
            _playerEventController.CursorMoved += CursorMovedHandler;
        }

        void Update()
        {
            foreach (var skill in _castable)
            {
                skill.Tick(Time.deltaTime);
            }
        }

        public SkillCastCommandReceivedEventArgs CreateCommandArgs(UnitSkill unitSkill, bool addToCommandsQueue = false)
        {
            if (!_castable.Contains(unitSkill))
            {
                throw new ArgumentException();
            }

            SkillCastCommandReceivedEventArgs args;

            var activeSkill = unitSkill.Skill as ActiveSkill;

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
            if (!_castable.Contains(unitSkill)) return false;
            if (unitSkill.CurrentCooldown > 0) return false;
            if (unitSkill.Skill is not ActiveSkill) return false;
            if (_unitValues.CurrentMana < (unitSkill.Skill as ActiveSkill).ManaCost) return false;

            return true;
        }

        public UnitSkill GetSkillByKeycode(KeyCode keycode)
        {
            return _castable.FirstOrDefault(x => x.Keycode == keycode);
        }

        private void BuildCastableSkills()
        {
            _castable.Clear();

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

                _castable.Add(new UnitSkill(slot));
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

            #region Banned keys
            private KeyCode[] banned = { KeyCode.Space, KeyCode.Escape, KeyCode.Mouse0, KeyCode.Mouse1, KeyCode.Mouse2, KeyCode.A, KeyCode.LeftShift };

            public bool IsBanned() => System.Array.IndexOf(banned, Keycode) >= 0;
            #endregion
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

            internal void StartCooldown(float cd)
            {
                CurrentCooldown = cd;
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
        }
    }
}
