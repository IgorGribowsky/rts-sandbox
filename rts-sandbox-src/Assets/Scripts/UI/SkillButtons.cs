using Assets.Scripts.GameObjects;
using Assets.Scripts.Infrastructure.Events;
using Assets.SkillsSection.Scripts;
using Assets.SkillsSection.Scripts.Events;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static Assets.SkillsSection.Scripts.UnitSkills;

namespace Assets.Scripts.UI
{
    /// <summary>
    /// The small round skill buttons on an arc around the command ring (M-023).
    /// One per skill of the main selected unit, passives included.
    ///
    /// State on the button: grey with seconds while on cooldown, dark blue when
    /// mana is short, no press for a passive. Lit while the skill is aimed, cast,
    /// or walked up to — by a key or by a click, it looks the same.
    /// </summary>
    public sealed class SkillButtons : IDisposable
    {
        private const long RefreshMilliseconds = 100;

        private readonly CommandRing _ring;
        private readonly CommandInput _commands;
        private readonly PlayerEventController _events;
        private readonly HudTooltip _tooltip;
        private readonly int _playerTeamId;
        private readonly List<Entry> _entries = new List<Entry>();
        private readonly IVisualElementScheduledItem _ticker;

        private UnitSkills _skills;
        private ManaValues _mana;
        private UnitEventManager _unitEvents;
        private UnitCommandManager _unitCommands;
        private bool _ownUnit;

        private sealed class Entry
        {
            public RingButton Button;
            public UnitSkill Skill;
            public CooldownSweep Sweep;
            public Label Seconds;
            public string State;
        }

        public SkillButtons(CommandRing ring, CommandInput commands, PlayerEventController events,
            HudTooltip tooltip, int playerTeamId)
        {
            _ring = ring;
            _commands = commands;
            _events = events;
            _tooltip = tooltip;
            _playerTeamId = playerTeamId;

            _events.SelectionChanged += OnSelectionChanged;
            _commands.ModesChanged += RefreshHighlights;
            _commands.HotkeyPressed += OnHotkey;

            // Cooldowns run on time, not on events, so the buttons look again ten
            // times a second — only while there is a unit with skills to look at.
            _ticker = _ring.Container.schedule.Execute(RefreshStates).Every(RefreshMilliseconds);
            _ticker.Pause();
        }

        public void Dispose()
        {
            _events.SelectionChanged -= OnSelectionChanged;
            _commands.ModesChanged -= RefreshHighlights;
            _commands.HotkeyPressed -= OnHotkey;
            _ticker.Pause();
            Unbind();
        }

        private void OnSelectionChanged(SelectionChangedEventArgs args)
        {
            Bind(args.MainUnit, args.TeamId);
        }

        private void Bind(GameObject unit, int teamId)
        {
            Unbind();

            _skills = unit != null ? unit.GetComponent<UnitSkills>() : null;
            if (_skills == null || _skills.RuntimeSkills.Count == 0)
            {
                return;
            }

            _ownUnit = teamId == _playerTeamId;
            _mana = unit.GetComponent<ManaValues>();
            _unitEvents = unit.GetComponent<UnitEventManager>();
            _unitCommands = unit.GetComponent<UnitCommandManager>();

            var count = _skills.RuntimeSkills.Count;
            for (var i = 0; i < count; i++)
            {
                _entries.Add(CreateEntry(i, count, _skills.RuntimeSkills[i]));
            }

            if (_unitEvents != null)
            {
                _unitEvents.ManaPointsChanged += OnManaChanged;
                _unitEvents.CurrentCommandChanged += OnCommandChanged;
            }

            RefreshStates();
            RefreshHighlights();
            _ticker.Resume();
        }

        private void Unbind()
        {
            _ticker?.Pause();
            _tooltip.Hide();

            if (_unitEvents != null)
            {
                _unitEvents.ManaPointsChanged -= OnManaChanged;
                _unitEvents.CurrentCommandChanged -= OnCommandChanged;
            }

            foreach (var entry in _entries)
            {
                entry.Button.RemoveFromHierarchy();
            }

            _entries.Clear();
            _skills = null;
            _mana = null;
            _unitEvents = null;
            _unitCommands = null;
        }

        /// <summary>
        /// Even spacing over the top left of the ring, from the left side to the
        /// top right. Few skills sit close together in the middle of the arc.
        /// </summary>
        public static float AngleOf(int index, int count)
        {
            const float middle = (CommandRing.SkillArcStart + CommandRing.SkillArcEnd) / 2f;
            const float span = CommandRing.SkillArcEnd - CommandRing.SkillArcStart;
            const float widestStep = 30f;

            if (count <= 1)
            {
                return middle;
            }

            var step = Mathf.Min(widestStep, span / (count - 1));
            return middle - step * (count - 1) / 2f + step * index;
        }

        private Entry CreateEntry(int index, int count, UnitSkill skill)
        {
            var button = new RingButton("ring-button--skill");
            button.SetIcon(skill.Skill.Icon);

            var isPassive = skill.Skill is PassiveSkill;
            button.EnableInClassList("is-passive", isPassive);
            button.Interactive = _ownUnit && !isPassive;

            var sweep = new CooldownSweep();
            sweep.AddToClassList("skill-sweep");
            button.Overlay.Add(sweep);

            var seconds = new Label { pickingMode = PickingMode.Ignore };
            seconds.AddToClassList("skill-seconds");
            seconds.AddToClassList("hud-text");
            button.Overlay.Add(seconds);

            var key = new Label(UiText.KeyName(skill.Keycode)) { pickingMode = PickingMode.Ignore };
            key.AddToClassList("skill-key");
            key.AddToClassList("hud-text");
            key.style.display = skill.Keycode == KeyCode.None ? DisplayStyle.None : DisplayStyle.Flex;
            button.Body.Add(key);

            CommandRing.Place(button, AngleOf(index, count), CommandRing.SkillRadius, CommandRing.SkillButtonSize);
            _ring.Container.Add(button);

            var entry = new Entry { Button = button, Skill = skill, Sweep = sweep, Seconds = seconds };

            button.Clicked += () => OnClicked(index);
            button.SecondaryPressed += _ => ShowTooltip(entry);
            button.SecondaryReleased += _ => _tooltip.Hide();

            return entry;
        }

        private void OnHotkey(HotkeyAction action, int index)
        {
            if (action == HotkeyAction.Skill && index >= 0 && index < _entries.Count)
            {
                _entries[index].Button.Pulse();
            }
        }

        private void OnClicked(int index)
        {
            // Not castable now (cooldown, mana) — nothing happens, as with a key.
            _commands.AimSkill(index);
        }

        // --- state ------------------------------------------------------------------

        private void OnManaChanged(ManaPointsChangedEventArgs args) => RefreshStates();

        private void OnCommandChanged(CurrentCommandChangedEventArgs args) => RefreshHighlights();

        private void RefreshStates()
        {
            if (_skills == null)
            {
                return;
            }

            foreach (var entry in _entries)
            {
                var skill = entry.Skill;
                string state;
                var cooldownFraction = 0f;

                if (skill.Skill is PassiveSkill)
                {
                    state = "passive";
                }
                else if (skill.CurrentCooldown > 0f)
                {
                    state = "cooldown";
                    cooldownFraction = skill.Skill.Cooldown > 0f ? skill.CurrentCooldown / skill.Skill.Cooldown : 0f;
                    entry.Seconds.text = UiText.CooldownLeft(skill.CurrentCooldown);
                }
                else if (skill.Skill is ActiveSkill active && (_mana == null || _mana.CurrentMana < active.ManaCost))
                {
                    state = "no-mana";
                }
                else
                {
                    state = "ready";
                }

                entry.Sweep.Fraction = cooldownFraction;

                if (state == entry.State)
                {
                    continue;
                }

                entry.State = state;
                entry.Button.EnableInClassList("is-cooldown", state == "cooldown");
                entry.Button.EnableInClassList("is-no-mana", state == "no-mana");
                entry.Seconds.style.display = state == "cooldown" ? DisplayStyle.Flex : DisplayStyle.None;
                entry.Button.SetIcon(state == "cooldown" ? GrayscaleCache.Get(skill.Skill.Icon) : skill.Skill.Icon);
            }
        }

        private void RefreshHighlights()
        {
            if (_skills == null)
            {
                return;
            }

            var aimed = _ownUnit ? _commands.AimedSkillIndex : -1;
            var running = _unitCommands != null ? _unitCommands.CurrentSkill : null;

            for (var i = 0; i < _entries.Count; i++)
            {
                var entry = _entries[i];
                entry.Button.SetActive(_ownUnit && (i == aimed || entry.Skill == running));
            }
        }

        // --- tooltip ------------------------------------------------------------

        private void ShowTooltip(Entry entry)
        {
            var skill = entry.Skill.Skill;
            var rows = new List<KeyValuePair<string, string>>();
            string kind;

            if (skill is ActiveSkill active)
            {
                kind = KindOf(active);
                rows.Add(new KeyValuePair<string, string>(UiText.Mana, active.ManaCost.ToString()));
                rows.Add(new KeyValuePair<string, string>(UiText.Cooldown, UiText.Seconds(skill.Cooldown)));
                // Cast at once (T-053): no cast time, no range to speak of.
                if (active.Action is not CastWithoutTargetAction)
                {
                    rows.Add(new KeyValuePair<string, string>(UiText.CastTime, UiText.Seconds(active.CastDuration)));
                    rows.Add(new KeyValuePair<string, string>(UiText.Range, UiText.Number(active.CastRange)));
                }

                if (active.Action is CastToAreaAction area)
                {
                    rows.Add(new KeyValuePair<string, string>(UiText.Radius, UiText.Number(area.AreaRadius)));
                }

                if (entry.Skill.Keycode != KeyCode.None)
                {
                    rows.Add(new KeyValuePair<string, string>(UiText.Key, UiText.KeyName(entry.Skill.Keycode)));
                }
            }
            else
            {
                kind = UiText.Passive;
            }

            _tooltip.Show(entry.Button, skill.Name, kind, skill.Description, rows, false);
        }

        private static string KindOf(ActiveSkill skill)
        {
            switch (skill.Action)
            {
                case CastToAreaAction _: return UiText.CastArea;
                case CastToTargetAction _: return UiText.CastUnit;
                case CastToPointAction _: return UiText.CastPoint;
                case CastWithoutTargetAction _: return UiText.CastAtOnce;
                default: return UiText.CastInstant;
            }
        }
    }
}
