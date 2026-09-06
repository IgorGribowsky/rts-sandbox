using Assets.Scripts;
using Assets.Scripts.Infrastructure.Events;
using Assets.SkillsSection.Scripts.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.SkillsSection.Scripts
{
    //There are a lot of architecture problems in this file. So it sholud be reworked in future.
    public class UnitSkills : MonoBehaviour
    {
        public List<UnitSkill> Skills;

        private PlayerEventController _playerEventController;

        private UnitValues _unitValues;

        private UnitEventManager _unitEventManager;

        private CursorMovedEventArgs _cursorMovedEventArgs;

        void Start()
        {
            _playerEventController = GameServices.PlayerEventController;
            _unitValues = GetComponent<UnitValues>();
            _unitEventManager = GetComponent<UnitEventManager>();
            _playerEventController.CursorMoved += CursorMovedHandler;

            SetupNavigationProperties();
        }

        void Update()
        {
            if (Skills == null)
            {
                return;
            }

            foreach (var skill in Skills)
            {
                if (skill == null) continue;

                skill.Tick(Time.deltaTime);
            }
        }

        public SkillCastCommandReceivedEventArgs CreateCommandArgs(UnitSkill unitSkill, bool addToCommandsQueue = false)
        {
            if (Skills == null || !Skills.Contains(unitSkill))
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
            var spendMana = (unitSkill.Skill as ActiveSkill).ManaCost;
            _unitEventManager.OnManaUsed(spendMana);
        }

        public bool CheckIfCanCast(UnitSkill unitSkill)
        {
            if (Skills == null || !Skills.Contains(unitSkill))
            {
                return false;
            }

            if (unitSkill == null) return false;
            if (unitSkill.CurrentCooldown > 0) return false;
            if (unitSkill.Skill is not ActiveSkill) return false;
            if (_unitValues.CurrentMana < (unitSkill.Skill as ActiveSkill).ManaCost) return false;

            return true;
        }

        public UnitSkill GetSkillByKeycode(KeyCode keycode)
        {
            if (Skills == null)
            {
                return null;
            }

            return Skills.FirstOrDefault(x => x.Keycode == keycode);
        }

        private void SetupNavigationProperties()
        {
            foreach (var unitSkill in Skills)
            {
                if (unitSkill.Skill is ActiveSkill)
                {
                    var activeSkill = (ActiveSkill)unitSkill.Skill;
                    activeSkill.Action.Skill = activeSkill;

                    foreach (var impact in activeSkill.Action.Impacts)
                    {
                        impact.SkillAction = activeSkill.Action;
                    }
                }
                else if (unitSkill.Skill is PassiveSkill)
                {
                    var passiveSkill = (PassiveSkill)unitSkill.Skill;
                    passiveSkill.Action.Skill = passiveSkill;

                    foreach (var impact in passiveSkill.Action.Impacts)
                    {
                        impact.SkillAction = passiveSkill.Action;
                    }
                }
            }
        }

        private void OnValidate()
        {
            if (Skills == null)
            {
                return;
            }

            foreach (var skill in Skills)
            {
                if (skill == null) continue;

                if (skill != null && skill.IsBanned())
                    skill.Keycode = KeyCode.None;
            }
        }

        protected void CursorMovedHandler(CursorMovedEventArgs args)
        {
            _cursorMovedEventArgs = args;
        }

        [Serializable]
        public class UnitSkill
        {
            public Skill Skill;

            public KeyCode Keycode;

            #region Skill logic
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
            #endregion

            #region Banned keys
            private KeyCode[] banned = { KeyCode.Space, KeyCode.Escape, KeyCode.Mouse0, KeyCode.Mouse1, KeyCode.Mouse2, KeyCode.A, KeyCode.LeftShift };

            public bool IsBanned() => System.Array.IndexOf(banned, Keycode) >= 0;
            #endregion
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
