using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts.UI
{
    /// <summary>
    /// A gauge drawn as a piece of a circle around the command ring (M-023):
    /// health, mana, experience. Angles are in degrees, clockwise from the
    /// right, because the panel's Y axis points down.
    ///
    /// Colours come from USS custom properties so that the theme keeps them:
    /// --gauge-fill, --gauge-back, --gauge-edge.
    /// </summary>
    public class ArcGauge : VisualElement
    {
        private static readonly CustomStyleProperty<Color> FillProperty = new CustomStyleProperty<Color>("--gauge-fill");
        private static readonly CustomStyleProperty<Color> BackProperty = new CustomStyleProperty<Color>("--gauge-back");
        private static readonly CustomStyleProperty<Color> EdgeProperty = new CustomStyleProperty<Color>("--gauge-edge");

        private Color _fill = Color.white;
        private Color _back = new Color(0f, 0f, 0f, 0.6f);
        private Color _edge = Color.black;

        private float _value = 1f;

        /// <summary>Distance from the centre of the element to the middle of the band.</summary>
        public float Radius { get; set; } = 90f;

        public float Thickness { get; set; } = 12f;

        public float StartAngle { get; set; }

        public float EndAngle { get; set; }

        /// <summary>The band fills from EndAngle back towards StartAngle.</summary>
        public bool FillFromEnd { get; set; }

        /// <summary>0..1 — how much of the band is filled.</summary>
        public float Value
        {
            get => _value;
            set
            {
                var clamped = Mathf.Clamp01(value);
                if (Mathf.Approximately(clamped, _value))
                {
                    return;
                }

                _value = clamped;
                MarkDirtyRepaint();
            }
        }

        public ArcGauge()
        {
            pickingMode = PickingMode.Ignore;
            generateVisualContent += Draw;
            RegisterCallback<CustomStyleResolvedEvent>(OnStyleResolved);
        }

        private void OnStyleResolved(CustomStyleResolvedEvent evt)
        {
            if (evt.customStyle.TryGetValue(FillProperty, out var fill)) _fill = fill;
            if (evt.customStyle.TryGetValue(BackProperty, out var back)) _back = back;
            if (evt.customStyle.TryGetValue(EdgeProperty, out var edge)) _edge = edge;
            MarkDirtyRepaint();
        }

        private void Draw(MeshGenerationContext context)
        {
            var center = new Vector2(contentRect.width / 2f, contentRect.height / 2f);
            var painter = context.painter2D;
            painter.lineCap = LineCap.Round;

            // Dark edge first, slightly wider, so the band reads on any ground.
            Stroke(painter, center, StartAngle, EndAngle, Thickness + 5f, _edge);
            Stroke(painter, center, StartAngle, EndAngle, Thickness, _back);

            if (_value <= 0f)
            {
                return;
            }

            var span = (EndAngle - StartAngle) * _value;
            if (FillFromEnd)
            {
                Stroke(painter, center, EndAngle - span, EndAngle, Thickness - 3f, _fill);
            }
            else
            {
                Stroke(painter, center, StartAngle, StartAngle + span, Thickness - 3f, _fill);
            }
        }

        private void Stroke(Painter2D painter, Vector2 center, float from, float to, float width, Color color)
        {
            if (to - from < 0.5f)
            {
                return;
            }

            painter.strokeColor = color;
            painter.lineWidth = width;
            painter.BeginPath();
            painter.Arc(center, Radius, Angle.Degrees(from), Angle.Degrees(to), ArcDirection.Clockwise);
            painter.Stroke();
        }
    }
}
