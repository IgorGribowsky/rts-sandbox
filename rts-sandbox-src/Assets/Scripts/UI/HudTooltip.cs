using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts.UI
{
    /// <summary>
    /// The card that opens while the right button is held on a skill or a
    /// card (M-022): name, kind, description and the numbers. One for the whole
    /// HUD; whoever opens it fills it.
    /// </summary>
    public sealed class HudTooltip
    {
        private const float Gap = 12f;

        private readonly VisualElement _root;
        private readonly VisualElement _panel;
        private readonly Label _title;
        private readonly Label _kind;
        private readonly Label _description;
        private readonly VisualElement _rows;

        public HudTooltip(VisualElement root)
        {
            _root = root;

            _panel = new VisualElement { pickingMode = PickingMode.Ignore };
            _panel.AddToClassList("tooltip");

            _title = new Label { pickingMode = PickingMode.Ignore };
            _title.AddToClassList("tooltip__title");
            _title.AddToClassList("hud-text");

            _kind = new Label { pickingMode = PickingMode.Ignore };
            _kind.AddToClassList("tooltip__kind");

            _description = new Label { pickingMode = PickingMode.Ignore };
            _description.AddToClassList("tooltip__description");

            _rows = new VisualElement { pickingMode = PickingMode.Ignore };
            _rows.AddToClassList("tooltip__rows");

            _panel.Add(_title);
            _panel.Add(_kind);
            _panel.Add(_description);
            _panel.Add(_rows);
            _root.Add(_panel);

            Hide();
        }

        /// <summary>
        /// Shows the card next to the element it describes: above it, aligned to
        /// its right edge for the ring on the right, to its left edge for cards
        /// on the left.
        /// </summary>
        public void Show(VisualElement anchor, string title, string kind, string description,
            IEnumerable<KeyValuePair<string, string>> rows, bool alignLeft)
        {
            _title.text = title;
            _kind.text = kind;
            _kind.style.display = string.IsNullOrEmpty(kind) ? DisplayStyle.None : DisplayStyle.Flex;
            _description.text = description;
            _description.style.display = string.IsNullOrEmpty(description) ? DisplayStyle.None : DisplayStyle.Flex;

            _rows.Clear();
            foreach (var row in rows)
            {
                var line = new VisualElement { pickingMode = PickingMode.Ignore };
                line.AddToClassList("tooltip__row");

                var name = new Label(row.Key) { pickingMode = PickingMode.Ignore };
                name.AddToClassList("tooltip__row-name");
                var value = new Label(row.Value) { pickingMode = PickingMode.Ignore };
                value.AddToClassList("tooltip__row-value");

                line.Add(name);
                line.Add(value);
                _rows.Add(line);
            }

            // Anchored by the side that faces the element, so the card's own size
            // does not matter: it grows away from the button.
            var bounds = _root.WorldToLocal(anchor.worldBound);
            var rootSize = _root.layout.size;

            _panel.style.bottom = Mathf.Max(Gap, rootSize.y - bounds.yMin + Gap);
            if (alignLeft)
            {
                _panel.style.left = Mathf.Max(Gap, bounds.xMin);
                _panel.style.right = StyleKeyword.Auto;
            }
            else
            {
                _panel.style.right = Mathf.Max(Gap, rootSize.x - bounds.xMax);
                _panel.style.left = StyleKeyword.Auto;
            }

            _panel.style.display = DisplayStyle.Flex;
            _panel.BringToFront();
        }

        public void Hide()
        {
            _panel.style.display = DisplayStyle.None;
        }
    }
}
