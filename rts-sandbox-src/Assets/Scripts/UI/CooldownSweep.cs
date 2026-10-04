using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts.UI
{
    /// <summary>
    /// A dark pie over a round button for the part of the cooldown still left,
    /// shrinking clockwise from the top like a clock (M-023). Colour from the
    /// USS custom property --sweep-color.
    /// </summary>
    public class CooldownSweep : VisualElement
    {
        private static readonly CustomStyleProperty<Color> ColorProperty = new CustomStyleProperty<Color>("--sweep-color");

        private Color _color = new Color(0f, 0f, 0f, 0.55f);
        private float _fraction;

        /// <summary>0..1 — how much of the cooldown is still left.</summary>
        public float Fraction
        {
            get => _fraction;
            set
            {
                var clamped = Mathf.Clamp01(value);
                if (Mathf.Abs(clamped - _fraction) < 0.002f)
                {
                    return;
                }

                _fraction = clamped;
                MarkDirtyRepaint();
            }
        }

        public CooldownSweep()
        {
            pickingMode = PickingMode.Ignore;
            generateVisualContent += Draw;
            RegisterCallback<CustomStyleResolvedEvent>(evt =>
            {
                if (evt.customStyle.TryGetValue(ColorProperty, out var color))
                {
                    _color = color;
                    MarkDirtyRepaint();
                }
            });
        }

        private void Draw(MeshGenerationContext context)
        {
            if (_fraction <= 0f)
            {
                return;
            }

            var rect = contentRect;
            var center = rect.center;
            var radius = Mathf.Min(rect.width, rect.height) / 2f;

            var painter = context.painter2D;
            painter.fillColor = _color;
            painter.BeginPath();

            if (_fraction >= 0.999f)
            {
                painter.Arc(center, radius, Angle.Degrees(0f), Angle.Degrees(360f));
            }
            else
            {
                // What is left runs from the current hand back round to the top.
                var start = -90f + (1f - _fraction) * 360f;
                painter.MoveTo(center);
                painter.Arc(center, radius, Angle.Degrees(start), Angle.Degrees(270f), ArcDirection.Clockwise);
                painter.ClosePath();
            }

            painter.Fill();
        }
    }
}
