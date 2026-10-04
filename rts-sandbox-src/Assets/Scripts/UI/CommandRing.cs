using Assets.Scripts.GameObjects;
using Assets.Scripts.GameObjects.UnitBehaviour;
using Assets.Scripts.Infrastructure.Enums;
using Assets.Scripts.Infrastructure.Events;
using Assets.SkillsSection.Scripts.Events;
using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts.UI
{
    /// <summary>
    /// The ring in the bottom right corner (M-023): what the main selected unit
    /// is and can do. Gauges of health and mana around a big attack button, the
    /// Hold button below on the left.
    ///
    /// Every button calls CommandInput — the same entry the hot keys use — and
    /// is lit by the unit's running order and the input mode, never by the
    /// fact that it was clicked. So a key and a click look the same.
    /// </summary>
    public sealed class CommandRing : IDisposable
    {
        // Ring geometry, in panel pixels. The ring is laid out around one centre
        // point inside its container; sizes of the parts are part of the layout,
        // the look of the parts lives in the theme.
        public const float CenterX = 216f;
        public const float CenterY = 218f;
        public const float BigButtonSize = 176f;
        public const float SmallButtonSize = 64f;
        public const float GaugeRadius = 106f;
        public const float GaugeThickness = 17f;
        public const float SmallButtonRadius = 128f;
        public const float SkillRadius = 182f;
        public const float SkillButtonSize = 72f;

        // Where the parts sit, in degrees clockwise from the right.
        public const float HoldAngle = 130f;
        public const float GatherAngle = 50f;
        public const float LevelAngle = 270f;
        public const float SkillArcStart = 195f;
        public const float SkillArcEnd = 335f;

        private readonly VisualElement _container;
        private readonly CommandInput _commands;
        private readonly PlayerEventController _events;
        private readonly int _playerTeamId;

        private readonly ArcGauge _hpGauge;
        private readonly ArcGauge _manaGauge;
        private readonly Label _hpLabel;
        private readonly Label _manaLabel;
        private readonly RingButton _attack;
        private readonly RingButton _hold;
        private readonly RingButton _gather;
        private readonly IVisualElementScheduledItem _gatherCheck;
        private bool _canGatherNow;

        private GameObject _unit;
        private UnitValues _values;
        private ManaValues _mana;
        private UnitEventManager _unitEvents;
        private UnitCommandManager _unitCommands;
        private bool _ownUnit;

        /// <summary>The bound unit, for the parts other presenters add to the ring.</summary>
        public GameObject Unit => _unit;

        public VisualElement Container => _container;

        public CommandRing(VisualElement container, CommandInput commands, PlayerEventController events,
            int playerTeamId)
        {
            _container = container;
            _commands = commands;
            _events = events;
            _playerTeamId = playerTeamId;

            // Built from scratch every time the HUD is: nothing left from before.
            _container.Clear();

            _hpGauge = CreateGauge("gauge--hp", 150f, 262f, false);
            _manaGauge = CreateGauge("gauge--mana", 278f, 390f, true);

            _attack = new RingButton("ring-button--big");
            _attack.AddToClassList("ring-attack");
            _attack.Clicked += OnAttackClicked;
            PlaceAtCenter(_attack, BigButtonSize);
            _container.Add(_attack);

            _hold = new RingButton("ring-button--small");
            _hold.AddToClassList("ring-hold");
            _hold.Clicked += OnHoldClicked;
            Place(_hold, HoldAngle, SmallButtonRadius, SmallButtonSize);
            _container.Add(_hold);

            _gather = new RingButton("ring-button--small");
            _gather.AddToClassList("ring-gather");
            _gather.Clicked += OnGatherClicked;
            Place(_gather, GatherAngle, SmallButtonRadius, SmallButtonSize);
            _container.Add(_gather);

            // Whether there is anything to gather changes as the worker walks and
            // trees run out; no event says so, so it is looked at twice a second.
            _gatherCheck = _container.schedule.Execute(RefreshGather).Every(500);
            _gatherCheck.Pause();

            // At the start of each band, low on the sides: the top of the ring
            // belongs to the skills.
            _hpLabel = CreateValueLabel("ring-value--hp", 158f);
            _manaLabel = CreateValueLabel("ring-value--mana", 22f);

            _events.SelectionChanged += OnSelectionChanged;
            _commands.ModesChanged += RefreshHighlights;

            Bind(null, 0);
        }

        public void Dispose()
        {
            _events.SelectionChanged -= OnSelectionChanged;
            _commands.ModesChanged -= RefreshHighlights;
            _gatherCheck.Pause();
            Unbind();
        }

        /// <summary>Puts an element so that its centre sits on the ring at this angle.</summary>
        public static void Place(VisualElement element, float angleDegrees, float radius, float size)
        {
            var radians = angleDegrees * Mathf.Deg2Rad;
            element.style.position = Position.Absolute;
            element.style.left = CenterX + Mathf.Cos(radians) * radius - size / 2f;
            element.style.top = CenterY + Mathf.Sin(radians) * radius - size / 2f;
            element.style.width = size;
            element.style.height = size;
        }

        public static void PlaceAtCenter(VisualElement element, float size)
        {
            Place(element, 0f, 0f, size);
        }

        private ArcGauge CreateGauge(string className, float start, float end, bool fromEnd)
        {
            var box = (GaugeRadius + GaugeThickness) * 2f;
            var gauge = new ArcGauge
            {
                Radius = GaugeRadius,
                Thickness = GaugeThickness,
                StartAngle = start,
                EndAngle = end,
                FillFromEnd = fromEnd,
            };
            gauge.AddToClassList("gauge");
            gauge.AddToClassList(className);
            PlaceAtCenter(gauge, box);
            _container.Add(gauge);
            return gauge;
        }

        private Label CreateValueLabel(string className, float angle)
        {
            var label = new Label { pickingMode = PickingMode.Ignore };
            label.AddToClassList("ring-value");
            label.AddToClassList("hud-text");
            label.AddToClassList(className);

            // Centred on the gauge band; the width is generous so long numbers fit.
            const float width = 112f;
            const float height = 28f;
            var radians = angle * Mathf.Deg2Rad;
            label.style.position = Position.Absolute;
            label.style.left = CenterX + Mathf.Cos(radians) * GaugeRadius - width / 2f;
            label.style.top = CenterY + Mathf.Sin(radians) * GaugeRadius - height / 2f;
            label.style.width = width;
            label.style.height = height;

            _container.Add(label);
            return label;
        }

        // --- binding ------------------------------------------------------------

        private void OnSelectionChanged(SelectionChangedEventArgs args)
        {
            Bind(args.MainUnit, args.TeamId);
        }

        private void Bind(GameObject unit, int teamId)
        {
            Unbind();

            _unit = unit;
            _ownUnit = unit != null && teamId == _playerTeamId;

            if (unit == null)
            {
                _container.style.display = DisplayStyle.None;
                return;
            }

            _values = unit.GetComponent<UnitValues>();
            _mana = unit.GetComponent<ManaValues>();
            _unitEvents = unit.GetComponent<UnitEventManager>();
            _unitCommands = unit.GetComponent<UnitCommandManager>();

            var type = _values != null ? _values.Type : null;
            var canAttack = type != null && (type.Behaviours.Contains(UnitBehaviourType.MeleeAttacking)
                || type.Behaviours.Contains(UnitBehaviourType.RangeAttacking)
                || type.Behaviours.Contains(UnitBehaviourType.AutoAttackBuilding));
            var hasSkills = type != null && type.HasSkills;

            // The ring is for units that fight or cast (M-023). A farm gets nothing.
            if (!canAttack && !hasSkills)
            {
                _container.style.display = DisplayStyle.None;
                return;
            }

            _container.style.display = DisplayStyle.Flex;

            _attack.Interactive = canAttack && _ownUnit;
            _attack.EnableInClassList("is-placeholder", !canAttack);

            var canHold = type.Behaviours.Contains(UnitBehaviourType.Holding);
            _hold.style.display = canHold ? DisplayStyle.Flex : DisplayStyle.None;
            _hold.Interactive = _ownUnit;

            var canGather = GatherTargets.CanGather(unit);
            _gather.style.display = canGather ? DisplayStyle.Flex : DisplayStyle.None;
            if (canGather)
            {
                RefreshGather();
                _gatherCheck.Resume();
            }

            var hasMana = _mana != null && _mana.MaximumMana > 0f;
            _manaGauge.style.display = hasMana ? DisplayStyle.Flex : DisplayStyle.None;
            _manaLabel.style.display = hasMana ? DisplayStyle.Flex : DisplayStyle.None;

            if (_unitEvents != null)
            {
                _unitEvents.HealthPointsChanged += OnHealthChanged;
                _unitEvents.ManaPointsChanged += OnManaChanged;
                _unitEvents.CurrentCommandChanged += OnCurrentCommandChanged;
            }

            RefreshHealth();
            RefreshMana();
            RefreshHighlights();
        }

        private void Unbind()
        {
            _gatherCheck?.Pause();

            if (_unitEvents != null)
            {
                _unitEvents.HealthPointsChanged -= OnHealthChanged;
                _unitEvents.ManaPointsChanged -= OnManaChanged;
                _unitEvents.CurrentCommandChanged -= OnCurrentCommandChanged;
            }

            _unit = null;
            _values = null;
            _mana = null;
            _unitEvents = null;
            _unitCommands = null;
        }

        // --- values ---------------------------------------------------------------

        private void OnHealthChanged(HealthPointsChangedEventArgs args) => RefreshHealth();

        private void OnManaChanged(ManaPointsChangedEventArgs args) => RefreshMana();

        private void RefreshHealth()
        {
            if (_values == null)
            {
                return;
            }

            var max = Mathf.Max(1f, _values.MaximumHp);
            var current = Mathf.Clamp(_values.CurrentHp, 0f, max);
            _hpGauge.Value = current / max;
            _hpLabel.text = UiText.Fraction(current, max);
        }

        private void RefreshMana()
        {
            if (_mana == null || _mana.MaximumMana <= 0f)
            {
                return;
            }

            var max = _mana.MaximumMana;
            var current = Mathf.Clamp(_mana.CurrentMana, 0f, max);
            _manaGauge.Value = current / max;
            _manaLabel.text = UiText.Fraction(current, max);
        }

        // --- what is happening now --------------------------------------------------

        private void OnCurrentCommandChanged(CurrentCommandChangedEventArgs args) => RefreshHighlights();

        private void RefreshHighlights()
        {
            var kind = _unitCommands != null ? _unitCommands.CurrentCommandKind : UnitCommandKind.Idle;

            _attack.SetActive(_ownUnit && (_commands.IsAClick
                || kind == UnitCommandKind.AMove || kind == UnitCommandKind.Attack));
            _hold.SetActive(_ownUnit && kind == UnitCommandKind.Hold);
            _gather.SetActive(_ownUnit && (kind == UnitCommandKind.Gather
                || kind == UnitCommandKind.Harvest || kind == UnitCommandKind.Mine));
        }

        /// <summary>Usable only when there is something to gather or somewhere to deliver (M-023).</summary>
        private void RefreshGather()
        {
            if (_unit == null)
            {
                return;
            }

            _canGatherNow = _ownUnit && GatherTargets.TryFind(_unit, out _);
            _gather.Interactive = _canGatherNow;
            _gather.EnableInClassList("is-unavailable", !_canGatherNow);
        }

        // --- clicks ----------------------------------------------------------------

        private void OnAttackClicked()
        {
            _commands.EnterAClick();
        }

        private void OnHoldClicked()
        {
            _commands.Hold(_commands.IsQueueModifierHeld);
        }

        private void OnGatherClicked()
        {
            _commands.Gather(_commands.IsQueueModifierHeld);
        }
    }
}
