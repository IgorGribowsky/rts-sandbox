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
    /// --gauge-fill, --gauge-back, --gauge-edge, --gauge-shine.
    ///
    /// A liquid gauge (T-058) looks like a liquid poured into the band: a small
    /// wave runs across its surface, a glint slides along it, and the level
    /// flows to a new value instead of jumping. The wave only decorates: the
    /// level is the value, and the wave never goes past the end of the band.
    /// It is redrawn only while it is on the screen.
    /// </summary>
    public class ArcGauge : VisualElement
    {
        private static readonly CustomStyleProperty<Color> FillProperty = new CustomStyleProperty<Color>("--gauge-fill");
        private static readonly CustomStyleProperty<Color> BackProperty = new CustomStyleProperty<Color>("--gauge-back");
        private static readonly CustomStyleProperty<Color> EdgeProperty = new CustomStyleProperty<Color>("--gauge-edge");
        private static readonly CustomStyleProperty<Color> ShineProperty = new CustomStyleProperty<Color>("--gauge-shine");

        private const long FrameMilliseconds = 33;
        private const float WavePixels = 2.6f;
        private const float WaveSpeed = 5.5f;
        private const float GlintPeriod = 2.8f;
        private const float GlintDegrees = 9f;
        private const float FlowPerSecond = 7f;
        private const int WaveSteps = 10;

        private Color _fill = Color.white;
        private Color _back = new Color(0f, 0f, 0f, 0.6f);
        private Color _edge = Color.black;
        private Color _shine = new Color(1f, 1f, 1f, 0.25f);

        private float _value = 1f;
        private float _shown = 1f;
        private bool _liquid;
        private IVisualElementScheduledItem _frames;
        private float _lastFrameTime;

        /// <summary>Distance from the centre of the element to the middle of the band.</summary>
        public float Radius { get; set; } = 90f;

        public float Thickness { get; set; } = 12f;

        public float StartAngle { get; set; }

        public float EndAngle { get; set; }

        /// <summary>The band fills from EndAngle back towards StartAngle.</summary>
        public bool FillFromEnd { get; set; }

        /// <summary>Wave, glint and a flowing level (T-058).</summary>
        public bool Liquid
        {
            get => _liquid;
            set
            {
                _liquid = value;
                if (_liquid && _frames == null)
                {
                    _frames = schedule.Execute(OnFrame).Every(FrameMilliseconds);
                }

                if (_liquid) _frames.Resume();
                else _frames?.Pause();
            }
        }

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
                if (!_liquid)
                {
                    _shown = _value;
                }

                MarkDirtyRepaint();
            }
        }

        /// <summary>Shows the value at once, without flowing: another unit was selected.</summary>
        public void Snap()
        {
            _shown = _value;
            MarkDirtyRepaint();
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
            if (evt.customStyle.TryGetValue(ShineProperty, out var shine)) _shine = shine;
            MarkDirtyRepaint();
        }

        private void OnFrame()
        {
            var now = Time.realtimeSinceStartup;
            var dt = Mathf.Clamp(now - _lastFrameTime, 0f, 0.1f);
            _lastFrameTime = now;

            if (!IsOnScreen())
            {
                return;
            }

            // The level flows to the value: fast at first, softly at the end.
            _shown = Mathf.Lerp(_shown, _value, 1f - Mathf.Exp(-FlowPerSecond * dt));
            if (Mathf.Abs(_shown - _value) < 0.001f)
            {
                _shown = _value;
            }

            MarkDirtyRepaint();
        }

        /// <summary>Hidden by itself or by any parent: nothing to redraw.</summary>
        private bool IsOnScreen()
        {
            if (panel == null)
            {
                return false;
            }

            for (VisualElement e = this; e != null; e = e.parent)
            {
                if (e.resolvedStyle.display == DisplayStyle.None || !e.visible)
                {
                    return false;
                }
            }

            return true;
        }

        private void Draw(MeshGenerationContext context)
        {
            var center = new Vector2(contentRect.width / 2f, contentRect.height / 2f);
            var painter = context.painter2D;
            painter.lineCap = LineCap.Round;

            // Dark edge first, slightly wider, so the band reads on any ground.
            Stroke(painter, center, StartAngle, EndAngle, Radius, Thickness + 5f, _edge);
            Stroke(painter, center, StartAngle, EndAngle, Radius, Thickness, _back);

            if (_shown <= 0.002f)
            {
                return;
            }

            var span = (EndAngle - StartAngle) * _shown;
            var width = Thickness - 3f;

            if (!_liquid || _shown >= 0.998f)
            {
                var from = FillFromEnd ? EndAngle - span : StartAngle;
                Stroke(painter, center, from, from + span, Radius, width, _fill);
                if (_liquid)
                {
                    DrawShine(painter, center, from, from + span, width, Time.realtimeSinceStartup);
                }

                return;
            }

            DrawLiquid(painter, center, span, width, Time.realtimeSinceStartup);
        }

        /// <summary>
        /// The filled part as one shape: a round cap where the band starts,
        /// two arcs, and a wavy surface where the liquid ends.
        /// </summary>
        private void DrawLiquid(Painter2D painter, Vector2 center, float span, float width, float time)
        {
            var d = FillFromEnd ? -1f : 1f;
            var anchor = FillFromEnd ? EndAngle : StartAngle;
            var surface = anchor + d * span;
            var forward = FillFromEnd ? ArcDirection.CounterClockwise : ArcDirection.Clockwise;
            var back = FillFromEnd ? ArcDirection.Clockwise : ArcDirection.CounterClockwise;

            var inner = Radius - width / 2f;
            var outer = Radius + width / 2f;

            // The wave is a few pixels, turned into degrees at this radius; it
            // fades out near the far end so it never spills over the band.
            var room = Mathf.Abs((FillFromEnd ? StartAngle : EndAngle) - surface);
            var amplitude = Mathf.Min(WavePixels / Radius * Mathf.Rad2Deg, room, span);

            float SurfaceAt(float radius)
            {
                var across = (radius - inner) / width;
                return surface + d * amplitude * Mathf.Sin((across * 2f + time * WaveSpeed / Mathf.PI) * Mathf.PI);
            }

            var cap = Point(center, anchor, Radius);

            painter.fillColor = _fill;
            painter.BeginPath();
            painter.MoveTo(Point(center, anchor, inner));
            painter.Arc(cap, width / 2f, Angle.Degrees(anchor + 180f), Angle.Degrees(anchor + 180f + d * 180f), forward);
            painter.Arc(center, outer, Angle.Degrees(anchor), Angle.Degrees(SurfaceAt(outer)), forward);
            for (var i = 1; i < WaveSteps; i++)
            {
                var radius = Mathf.Lerp(outer, inner, i / (float)WaveSteps);
                painter.LineTo(Point(center, SurfaceAt(radius), radius));
            }
            painter.LineTo(Point(center, SurfaceAt(inner), inner));
            painter.Arc(center, inner, Angle.Degrees(SurfaceAt(inner)), Angle.Degrees(anchor), back);
            painter.ClosePath();
            painter.Fill();

            // Shine stays clear of the wave.
            var shineEnd = surface - d * (amplitude + 1.5f);
            if ((shineEnd - anchor) * d > 0f)
            {
                DrawShine(painter, center, Mathf.Min(anchor, shineEnd), Mathf.Max(anchor, shineEnd), width, time);
            }
        }

        /// <summary>A thin gloss along the outer side and a glint sliding along the liquid.</summary>
        private void DrawShine(Painter2D painter, Vector2 center, float from, float to, float width, float time)
        {
            Stroke(painter, center, from, to, Radius + width * 0.24f, width * 0.16f, _shine);

            var length = to - from;
            if (length < GlintDegrees)
            {
                return;
            }

            var phase = Mathf.Repeat(time / GlintPeriod, 1f);
            var middle = from + GlintDegrees / 2f + (length - GlintDegrees) * phase;
            var glint = _shine;
            glint.a *= Mathf.Sin(phase * Mathf.PI) * 1.6f;
            Stroke(painter, center, middle - GlintDegrees / 2f, middle + GlintDegrees / 2f, Radius + width * 0.05f, width * 0.32f, glint);
        }

        private static Vector2 Point(Vector2 center, float degrees, float radius)
        {
            var radians = degrees * Mathf.Deg2Rad;
            return center + new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)) * radius;
        }

        private static void Stroke(Painter2D painter, Vector2 center, float from, float to, float radius, float width, Color color)
        {
            if (to - from < 0.5f || color.a <= 0.001f)
            {
                return;
            }

            painter.strokeColor = color;
            painter.lineWidth = width;
            painter.BeginPath();
            painter.Arc(center, radius, Angle.Degrees(from), Angle.Degrees(to), ArcDirection.Clockwise);
            painter.Stroke();
        }
    }
}
