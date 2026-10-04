using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts.UI
{
    /// <summary>
    /// A thin ring that fills clockwise from the top: how far a unit in
    /// production has got (M-024). Drawn along the rim of a round button.
    ///
    /// Colours from USS custom properties: --progress-fill, --progress-back.
    /// Radius and thickness are fractions of the element's size, so the ring
    /// follows the button when it grows or shrinks.
    /// </summary>
    public class ProgressRing : VisualElement
    {
        private static readonly CustomStyleProperty<Color> FillProperty = new CustomStyleProperty<Color>("--progress-fill");
        private static readonly CustomStyleProperty<Color> BackProperty = new CustomStyleProperty<Color>("--progress-back");

        private Color _fill = Color.white;
        private Color _back = new Color(0f, 0f, 0f, 0.6f);
        private float _value;

        /// <summary>Middle of the band, as a part of half the element's size.</summary>
        public float RadiusFraction { get; set; } = 0.87f;

        /// <summary>Width of the band, as a part of the element's size.</summary>
        public float ThicknessFraction { get; set; } = 0.09f;

        /// <summary>0..1 — how much of the ring is filled.</summary>
        public float Value
        {
            get => _value;
            set
            {
                var clamped = Mathf.Clamp01(value);
                if (Mathf.Abs(clamped - _value) < 0.002f)
                {
                    return;
                }

                _value = clamped;
                MarkDirtyRepaint();
            }
        }

        public ProgressRing()
        {
            pickingMode = PickingMode.Ignore;
            generateVisualContent += Draw;
            RegisterCallback<CustomStyleResolvedEvent>(evt =>
            {
                if (evt.customStyle.TryGetValue(FillProperty, out var fill)) _fill = fill;
                if (evt.customStyle.TryGetValue(BackProperty, out var back)) _back = back;
                MarkDirtyRepaint();
            });
        }

        private void Draw(MeshGenerationContext context)
        {
            var rect = contentRect;
            var size = Mathf.Min(rect.width, rect.height);
            if (size <= 0f)
            {
                return;
            }

            var center = rect.center;
            var radius = size / 2f * RadiusFraction;
            var width = size * ThicknessFraction;

            var painter = context.painter2D;
            painter.lineCap = LineCap.Butt;

            painter.strokeColor = _back;
            painter.lineWidth = width;
            painter.BeginPath();
            painter.Arc(center, radius, Angle.Degrees(0f), Angle.Degrees(360f));
            painter.Stroke();

            if (_value <= 0.001f)
            {
                return;
            }

            painter.lineCap = LineCap.Round;
            painter.strokeColor = _fill;
            painter.lineWidth = width * 0.8f;
            painter.BeginPath();
            painter.Arc(center, radius, Angle.Degrees(-90f), Angle.Degrees(-90f + 360f * _value), ArcDirection.Clockwise);
            painter.Stroke();
        }
    }
}
