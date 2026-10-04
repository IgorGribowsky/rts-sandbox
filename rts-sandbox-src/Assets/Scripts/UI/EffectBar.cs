using Assets.Scripts.Infrastructure.Events;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts.UI
{
    /// <summary>
    /// Effects on the main selected unit (T-073): a row of small round badges
    /// to the left of the command ring, the first to land nearest to it. The picture
    /// comes from the effect's EffectInfo, or else from the skill that put the
    /// effect on. Under each badge — the seconds it has left; the badge darkens
    /// clockwise as the time runs out. The rim tells help from harm. The right
    /// button held on a badge opens the tooltip, as on a skill (M-022).
    ///
    /// The time runs without events, so the row looks again ten times a second
    /// while a unit is selected. The list itself is rebuilt on UnitEffects.Changed.
    /// </summary>
    public sealed class EffectBar : IDisposable
    {
        private const long RefreshMilliseconds = 100;

        private readonly VisualElement _bar;
        private readonly PlayerEventController _events;
        private readonly HudTooltip _tooltip;
        private readonly IVisualElementScheduledItem _ticker;
        private readonly List<Entry> _entries = new List<Entry>();

        private GameObject _unit;
        private UnitEffects _effects;
        private bool _dirty;

        private sealed class Entry
        {
            public UnitEffect Effect;
            public VisualElement Root;
            public RingButton Button;
            public CooldownSweep Sweep;
            public Label Time;
        }

        public EffectBar(VisualElement bar, PlayerEventController events, HudTooltip tooltip)
        {
            _bar = bar;
            _events = events;
            _tooltip = tooltip;

            _bar.Clear();
            _events.SelectionChanged += OnSelectionChanged;

            _ticker = _bar.schedule.Execute(Refresh).Every(RefreshMilliseconds);
            _ticker.Pause();
        }

        public void Dispose()
        {
            _events.SelectionChanged -= OnSelectionChanged;
            _ticker.Pause();
            Unbind();
        }

        private void OnSelectionChanged(SelectionChangedEventArgs args)
        {
            Unbind();

            _unit = args.MainUnit;
            if (_unit == null)
            {
                return;
            }

            _dirty = true;
            Refresh();
            _ticker.Resume();
        }

        private void Unbind()
        {
            _ticker?.Pause();

            if (_effects != null)
            {
                _effects.Changed -= OnEffectsChanged;
            }

            _effects = null;
            _unit = null;
            Clear();
        }

        private void OnEffectsChanged()
        {
            _dirty = true;
        }

        private void Refresh()
        {
            if (_unit == null)
            {
                // Died while selected: whatever showed goes with it.
                if (_entries.Count > 0)
                {
                    Clear();
                }

                return;
            }

            // The holder appears with the first effect that lands, so a unit
            // selected clean gets it some time later.
            if (_effects == null)
            {
                _effects = _unit.GetComponent<UnitEffects>();
                if (_effects == null)
                {
                    return;
                }

                _effects.Changed += OnEffectsChanged;
                _dirty = true;
            }

            if (_dirty)
            {
                _dirty = false;
                Rebuild();
            }

            foreach (var entry in _entries)
            {
                var effect = entry.Effect;
                entry.Time.text = UiText.EffectLeft(effect.Remaining);
                entry.Sweep.Fraction = effect.Duration > 0f ? 1f - effect.Remaining / effect.Duration : 0f;
            }
        }

        private void Rebuild()
        {
            Clear();

            foreach (var effect in _effects.All)
            {
                _entries.Add(CreateEntry(effect));
            }
        }

        private void Clear()
        {
            _tooltip.Hide();

            foreach (var entry in _entries)
            {
                entry.Root.RemoveFromHierarchy();
            }

            _entries.Clear();
        }

        private Entry CreateEntry(UnitEffect effect)
        {
            var root = new VisualElement { pickingMode = PickingMode.Ignore };
            root.AddToClassList("effect");
            root.AddToClassList(IsBuff(effect) ? "effect--buff" : "effect--debuff");

            // Shown, never pressed: a click on it means nothing but must not
            // reach the world either.
            var button = new RingButton("ring-button--effect") { Interactive = false };
            button.SetIcon(IconOf(effect));

            var sweep = new CooldownSweep();
            sweep.AddToClassList("effect-sweep");
            button.Overlay.Add(sweep);

            var time = new Label { pickingMode = PickingMode.Ignore };
            time.AddToClassList("effect-time");
            time.AddToClassList("hud-text");

            root.Add(button);
            root.Add(time);
            _bar.Add(root);

            var entry = new Entry { Effect = effect, Root = root, Button = button, Sweep = sweep, Time = time };

            button.SecondaryPressed += _ => ShowTooltip(entry);
            button.SecondaryReleased += _ => _tooltip.Hide();

            return entry;
        }

        private void ShowTooltip(Entry entry)
        {
            var effect = entry.Effect;
            var rows = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>(UiText.TimeLeft, UiText.Seconds(Mathf.Ceil(effect.Remaining * 10f) / 10f)),
            };

            switch (effect)
            {
                case StatBoostEffect boost:
                    if (boost.DamagePercent != 0f)
                        rows.Add(new KeyValuePair<string, string>(UiText.DamageBonus, UiText.Percent(boost.DamagePercent)));
                    if (boost.AttackSpeedPercent != 0f)
                        rows.Add(new KeyValuePair<string, string>(UiText.AttackSpeedBonus, UiText.Percent(boost.AttackSpeedPercent)));
                    break;
                case PoisonEffect poison:
                    rows.Add(new KeyValuePair<string, string>(UiText.DamagePerSecond, UiText.Number(poison.Dps)));
                    break;
            }

            var skill = SkillCatalog.SkillOf(effect.Key);
            if (skill != null && InfoOf(effect) != null)
            {
                // The badge has a picture of its own; say which skill it came from.
                rows.Add(new KeyValuePair<string, string>(UiText.AppliedBy, skill.Name));
            }

            _tooltip.Show(entry.Button, NameOf(effect), IsBuff(effect) ? UiText.Buff : UiText.Debuff,
                DescriptionOf(effect), rows, false);
        }

        // --- what the effect looks like -----------------------------------------------

        private static EffectInfo InfoOf(UnitEffect effect)
        {
            var info = (effect.Key as IEffectImpact)?.EffectInfo;
            return info != null ? info : null;
        }

        private static Texture2D IconOf(UnitEffect effect)
        {
            var info = InfoOf(effect);
            if (info != null && info.Icon != null)
            {
                return info.Icon;
            }

            return SkillCatalog.SkillOf(effect.Key)?.Icon;
        }

        private static string NameOf(UnitEffect effect)
        {
            var info = InfoOf(effect);
            if (info != null && !string.IsNullOrEmpty(info.Name))
            {
                return info.Name;
            }

            var skill = SkillCatalog.SkillOf(effect.Key);
            if (skill != null && !string.IsNullOrEmpty(skill.Name))
            {
                return skill.Name;
            }

            switch (effect)
            {
                case StunEffect _: return "Stunned";
                case PoisonEffect _: return "Poisoned";
                case StatBoostEffect _: return "Boosted";
                default: return effect.GetType().Name;
            }
        }

        private static string DescriptionOf(UnitEffect effect)
        {
            var info = InfoOf(effect);
            if (info != null && !string.IsNullOrEmpty(info.Description))
            {
                return info.Description;
            }

            return SkillCatalog.SkillOf(effect.Key)?.Description;
        }

        private static bool IsBuff(UnitEffect effect)
        {
            return effect is StatBoostEffect;
        }
    }
}
