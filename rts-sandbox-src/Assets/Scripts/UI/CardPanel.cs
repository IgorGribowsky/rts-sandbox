using Assets.Scripts.Infrastructure.Events;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts.UI
{
    /// <summary>
    /// The bottom left corner (M-024). A builder gets the build button, and
    /// while the menu is open, a row of building cards. The menu is opened by
    /// the button or `B` alike: the panel follows the mode, it does not keep
    /// one of its own.
    ///
    /// A finished building that trains units shows its hire cards all the
    /// time, with no button: a click is the same as the unit's digit key.
    ///
    /// The row never grows over the command ring; what does not fit scrolls
    /// by dragging.
    /// </summary>
    public sealed class CardPanel : IDisposable
    {
        private readonly CommandInput _commands;
        private readonly PlayerEventController _events;
        private readonly PlayerResources _playerResources;
        private readonly GameResources _gameResources;
        private readonly HudTooltip _tooltip;
        private readonly int _playerTeamId;

        private readonly RingButton _buildButton;
        private readonly ScrollView _strip;
        private readonly List<UnitCard> _cards = new List<UnitCard>();
        private readonly Dictionary<int, UnitCard> _hireCards = new Dictionary<int, UnitCard>();

        private BuilderValues _builder;
        private BuildingValues _producer;
        private Building _producerBuilding;
        private UnitEventManager _producerEvents;

        // What the row shows now and for whom; rebuilt only when either changes.
        private string _content = "";
        private UnityEngine.Object _contentOwner;

        public CardPanel(VisualElement buttonSlot, ScrollView strip, CommandInput commands,
            PlayerEventController events, PlayerResources playerResources, GameResources gameResources,
            HudTooltip tooltip, int playerTeamId)
        {
            _commands = commands;
            _events = events;
            _playerResources = playerResources;
            _gameResources = gameResources;
            _tooltip = tooltip;
            _playerTeamId = playerTeamId;
            UnitPreviewRenderer.TeamId = playerTeamId;

            buttonSlot.Clear();
            _buildButton = new RingButton("ring-button--build");
            _buildButton.AddToClassList("build-button");
            _buildButton.Clicked += () => _commands.ToggleBuildMenu();
            buttonSlot.Add(_buildButton);

            _strip = strip;
            _strip.mode = ScrollViewMode.Horizontal;
            _strip.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
            _strip.verticalScrollerVisibility = ScrollerVisibility.Hidden;
            _strip.AddManipulator(new DragScroller(_strip));

            _events.SelectionChanged += OnSelectionChanged;
            _events.ResourceChanged += OnResourceChanged;
            _commands.ModesChanged += Refresh;
            _commands.HotkeyPressed += OnHotkey;

            Bind(null, 0);
        }

        public void Dispose()
        {
            UnbindProducer();
            _events.SelectionChanged -= OnSelectionChanged;
            _events.ResourceChanged -= OnResourceChanged;
            _commands.ModesChanged -= Refresh;
            _commands.HotkeyPressed -= OnHotkey;
            ClearCards();
        }

        private void OnSelectionChanged(SelectionChangedEventArgs args)
        {
            Bind(args.MainUnit, args.TeamId);
        }

        private void Bind(GameObject unit, int teamId)
        {
            var own = unit != null && teamId == _playerTeamId;
            var builder = own ? unit.GetComponent<BuilderValues>() : null;
            _builder = builder != null && builder.IsBuilder ? builder : null;

            UnbindProducer();
            var producer = own ? unit.GetComponent<BuildingValues>() : null;
            if (producer != null && producer.CanProduceUnits && producer.UnitsToProduce.Count > 0)
            {
                _producer = producer;
                _producerBuilding = unit.GetComponent<Building>();
                _producerEvents = unit.GetComponent<UnitEventManager>();
                if (_producerEvents != null)
                {
                    _producerEvents.BuildingCompleted += OnProducerCompleted;
                }
            }

            _buildButton.style.display = _builder != null ? DisplayStyle.Flex : DisplayStyle.None;
            Refresh();
        }

        private void UnbindProducer()
        {
            if (_producerEvents != null)
            {
                _producerEvents.BuildingCompleted -= OnProducerCompleted;
            }

            _producer = null;
            _producerBuilding = null;
            _producerEvents = null;
        }

        private void OnProducerCompleted(BuildingCompletedEventArgs args) => Refresh();

        private void Refresh()
        {
            _buildButton.SetActive(_builder != null && (_commands.IsBuildMenuOpen || _commands.IsPlacingBuilding));

            string content;
            UnityEngine.Object owner;
            if (_builder != null && _commands.IsBuildMenuOpen)
            {
                content = "buildings";
                owner = _builder;
            }
            else if (_producer != null && (_producerBuilding == null || !_producerBuilding.BuildingIsInProgress))
            {
                // A building still going up cannot train anybody yet (M-011).
                content = "hire";
                owner = _producer;
            }
            else
            {
                content = "";
                owner = null;
            }

            if (content == _content && owner == _contentOwner)
            {
                return;
            }

            _content = content;
            _contentOwner = owner;
            ClearCards();

            if (content == "hire")
            {
                for (var i = 0; i < _producer.UnitsToProduce.Count; i++)
                {
                    var type = _producer.UnitsToProduce[i];
                    if (type == null)
                    {
                        continue;
                    }

                    // Same numbering as the digit keys: 1..9, then 0 for the tenth.
                    var index = i;
                    var key = index < 10 ? ((index + 1) % 10).ToString() : "";
                    var card = new UnitCard(type, key, _gameResources);
                    card.Clicked += () => _commands.Produce(index);
                    AddCard(card);
                    _hireCards[index] = card;
                }

                _strip.scrollOffset = Vector2.zero;
            }
            else if (content == "buildings")
            {
                foreach (var entry in _builder.BuildingsToProduce)
                {
                    if (entry == null || entry.Building == null)
                    {
                        continue;
                    }

                    var key = entry.KeyCode;
                    var card = new UnitCard(entry.Building, UiText.KeyName(key), _gameResources);
                    card.Clicked += () => _commands.ChooseBuilding(key);
                    AddCard(card);
                }

                _strip.scrollOffset = Vector2.zero;
            }

            _strip.style.display = _cards.Count > 0 ? DisplayStyle.Flex : DisplayStyle.None;
            RefreshAffordability();
        }

        /// <summary>
        /// B shows on the build button, a digit on its hire card: the same
        /// press animation a click gives (M-022).
        /// </summary>
        private void OnHotkey(HotkeyAction action, int index)
        {
            if (action == HotkeyAction.BuildMenu && _builder != null)
            {
                _buildButton.Pulse();
            }
            else if (action == HotkeyAction.Produce && _content == "hire" && _hireCards.TryGetValue(index, out var card))
            {
                card.Pulse();
            }
        }

        private void AddCard(UnitCard card)
        {
            card.SecondaryPressed += ShowTooltip;
            card.SecondaryReleased += _ => _tooltip.Hide();
            _strip.Add(card);
            _cards.Add(card);
        }

        private void ClearCards()
        {
            _tooltip.Hide();
            foreach (var card in _cards)
            {
                card.RemoveFromHierarchy();
            }

            _cards.Clear();
            _hireCards.Clear();
        }

        private void OnResourceChanged(ResourceChangedEventArgs args) => RefreshAffordability();

        private void RefreshAffordability()
        {
            foreach (var card in _cards)
            {
                card.RefreshAffordability(_playerResources, _gameResources);
            }
        }

        private void ShowTooltip(UnitCard card)
        {
            var type = card.Type;
            var kind = type.IsBuildingObject ? UiText.Building : UiText.Unit;
            _tooltip.Show(card, type.DisplayName, kind, type.Description, UnitCard.TooltipRows(type), true);
        }
    }
}
