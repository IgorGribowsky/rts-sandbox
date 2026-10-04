using Assets.Scripts.Infrastructure.Enums;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts.UI
{
    /// <summary>
    /// The in-game HUD (M-022). Owns the UIDocument and hands its parts to the
    /// small presenters that draw them; it does not draw anything itself.
    ///
    /// The HUD is not a screen: it lives for the whole game and only shows and
    /// hides its parts. It never gives orders on its own either — every button
    /// goes through the same entry points the keyboard uses.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class HudController : MonoBehaviour
    {
        private UIDocument _document;
        private VisualElement _root;

        private PlayerResources _playerResources;
        private PlayerEventController _playerEventController;
        private GameResources _gameResources;
        private CommandInput _commands;
        private int _playerTeamId;

        private ResourcePanel _resourcePanel;
        private CommandRing _commandRing;
        private SkillButtons _skillButtons;
        private CardPanel _cardPanel;
        private HudTooltip _tooltip;

        /// <summary>The root of the HUD tree, null until the document is up.</summary>
        public VisualElement Root => _root;

        private void Awake()
        {
            _document = GetComponent<UIDocument>();

            var player = GameObject.FindGameObjectWithTag(Tag.PlayerController.ToString());
            _playerResources = player.GetComponent<PlayerResources>();
            _playerEventController = player.GetComponent<PlayerEventController>();
            _commands = player.GetComponent<CommandInput>();
            _playerTeamId = player.GetComponent<PlayerTeamMember>().TeamId;

            var gameController = GameObject.FindGameObjectWithTag(Tag.GameController.ToString());
            _gameResources = gameController.GetComponent<GameResources>();
        }

        private void OnEnable()
        {
            // UIDocument rebuilds its tree in its own OnEnable, which may run
            // after this one; the tree is taken in Start and re-taken here
            // when the component is switched back on later.
            if (_root != null)
            {
                Build();
            }
        }

        private void Start()
        {
            Build();
        }

        private void OnDisable()
        {
            _resourcePanel?.Dispose();
            _resourcePanel = null;
            _skillButtons?.Dispose();
            _skillButtons = null;
            _cardPanel?.Dispose();
            _cardPanel = null;
            _commandRing?.Dispose();
            _commandRing = null;
        }

        private void Build()
        {
            _root = _document.rootVisualElement;

            _resourcePanel?.Dispose();
            _resourcePanel = new ResourcePanel(
                _root.Q<VisualElement>("resource-bar"),
                _gameResources,
                _playerResources,
                _playerEventController);

            var hudRoot = _root.Q<VisualElement>("hud-root");
            hudRoot.Q<VisualElement>(className: "tooltip")?.RemoveFromHierarchy();
            _tooltip = new HudTooltip(hudRoot);

            _skillButtons?.Dispose();
            _commandRing?.Dispose();
            _commandRing = new CommandRing(
                _root.Q<VisualElement>("command-ring"),
                _commands,
                _playerEventController,
                _playerTeamId);
            _skillButtons = new SkillButtons(_commandRing, _commands, _playerEventController, _tooltip, _playerTeamId);

            _cardPanel?.Dispose();
            _cardPanel = new CardPanel(
                _root.Q<VisualElement>("build-button-slot"),
                _root.Q<ScrollView>("card-strip"),
                _commands,
                _playerEventController,
                _playerResources,
                _gameResources,
                _tooltip,
                _playerTeamId);
        }

        /// <summary>
        /// Is the pointer over a part of the HUD that takes clicks? The world
        /// must not get a click that was meant for a button (M-022).
        /// </summary>
        public bool IsPointerOverUI(Vector2 screenPosition)
        {
            if (_root?.panel == null)
            {
                return false;
            }

            // Screen space has its origin at the bottom left, panels at the top left.
            var flipped = new Vector2(screenPosition.x, Screen.height - screenPosition.y);
            var panelPosition = RuntimePanelUtils.ScreenToPanel(_root.panel, flipped);
            var picked = _root.panel.Pick(panelPosition);

            return picked != null;
        }
    }
}
