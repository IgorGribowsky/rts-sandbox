using System;

namespace RtsSandbox.Rules
{
    /// <summary>
    /// What the player knows about one cell of the map (M-027), in the order
    /// of how much is known. Seen once, a cell never goes back to black.
    /// </summary>
    public enum FogState : byte
    {
        /// <summary>Black: never seen, not even the ground.</summary>
        Unexplored = 0,

        /// <summary>Grey: seen before, not seen now.</summary>
        Explored = 1,

        /// <summary>Clear: someone on the player's side sees it right now.</summary>
        Visible = 2,
    }

    /// <summary>
    /// The fog of war as a grid of cells (M-027, T-071.1). Positions and radii
    /// are in cells: the caller turns metres into cells.
    ///
    /// One pass of sight is <see cref="BeginPass"/>, then one
    /// <see cref="Reveal"/> per unit that sees. Everything not revealed in
    /// the pass stays as it was, except that what was visible turns grey.
    ///
    /// Besides the state every cell has a light, 0..1: how clearly it is seen
    /// this pass. It fades to nothing over the last stretch of the radius, so
    /// the picture has a soft edge and not a staircase of cells. The state
    /// alone decides what is seen; the light is only for the picture.
    /// </summary>
    public sealed class FogGrid
    {
        private readonly FogState[] _cells;
        private readonly float[] _light;

        public FogGrid(int width, int height)
        {
            if (width <= 0 || height <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(width), "The grid needs at least one cell.");
            }

            Width = width;
            Height = height;
            _cells = new FogState[width * height];
            _light = new float[width * height];
        }

        public int Width { get; }

        public int Height { get; }

        public FogState this[int x, int y] => _cells[y * Width + x];

        /// <summary>Row by row from the bottom left. Read only — do not hold on to it.</summary>
        public FogState[] Cells => _cells;

        /// <summary>The light of every cell, same order. Read only — do not hold on to it.</summary>
        public float[] Light => _light;

        /// <summary>A new pass of sight: whatever was seen is now only remembered.</summary>
        public void BeginPass()
        {
            for (var i = 0; i < _cells.Length; i++)
            {
                if (_cells[i] == FogState.Visible)
                {
                    _cells[i] = FogState.Explored;
                }

                _light[i] = 0f;
            }
        }

        /// <summary>
        /// Everything within the radius of the point becomes visible. A cell
        /// counts when its centre is inside the circle. The point may lie off
        /// the grid: only the part of the circle on the grid is revealed.
        ///
        /// The light is full up to radius minus feather and falls to zero at
        /// the radius. Where two circles overlap the brighter one wins.
        /// </summary>
        public void Reveal(float x, float y, float radius, float feather = 0f)
        {
            if (radius <= 0f)
            {
                return;
            }

            var xMin = Math.Max(0, (int)Math.Floor(x - radius));
            var xMax = Math.Min(Width - 1, (int)Math.Ceiling(x + radius));
            var yMin = Math.Max(0, (int)Math.Floor(y - radius));
            var yMax = Math.Min(Height - 1, (int)Math.Ceiling(y + radius));
            var radiusSquared = radius * radius;
            feather = Math.Min(feather, radius);
            var fullSquared = (radius - feather) * (radius - feather);

            for (var cy = yMin; cy <= yMax; cy++)
            {
                var dy = cy + 0.5f - y;
                var row = cy * Width;

                for (var cx = xMin; cx <= xMax; cx++)
                {
                    var dx = cx + 0.5f - x;
                    var distanceSquared = dx * dx + dy * dy;
                    if (distanceSquared > radiusSquared)
                    {
                        continue;
                    }

                    var i = row + cx;
                    _cells[i] = FogState.Visible;

                    var light = distanceSquared <= fullSquared
                        ? 1f
                        : (radius - (float)Math.Sqrt(distanceSquared)) / feather;
                    if (light > _light[i])
                    {
                        _light[i] = light;
                    }
                }
            }
        }
    }
}
