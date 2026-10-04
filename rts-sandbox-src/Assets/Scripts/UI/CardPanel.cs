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

        private BuilderValues _builder;
        private bool _showingBuildings;

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

            Bind(null, 0);
        }

        public void Dispose()
        {
            _events.SelectionChanged -= OnSelectionChanged;
            _events.ResourceChanged -= OnResourceChanged;
            _commands.ModesChanged -= Refresh;
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

            _buildButton.style.display = _builder != null ? DisplayStyle.Flex : DisplayStyle.None;
            Refresh();
        }

        private void Refresh()
        {
            var showBuildings = _builder != null && _commands.IsBuildMenuOpen;
            _buildButton.SetActive(_builder != null && (_commands.IsBuildMenuOpen || _commands.IsPlacingBuilding));

            if (showBuildings == _showingBuildings)
            {
                return;
            }

            _showingBuildings = showBuildings;
            ClearCards();

            if (showBuildings)
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
