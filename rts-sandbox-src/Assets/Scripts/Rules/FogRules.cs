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

    /// <summary>What the player is shown of one unit under the fog (M-027, T-071.3).</summary>
    public enum FogSight : byte
    {
        /// <summary>Drawn, on the minimap, can be clicked, selected, attacked.</summary>
        Visible = 0,

        /// <summary>
        /// A building seen before and not seen now: drawn as it was, on the
        /// minimap, but cannot be clicked or selected.
        /// </summary>
        Remembered = 1,

        /// <summary>Not drawn, not on the minimap, cannot be clicked. Still plays on as usual.</summary>
        Hidden = 2,
    }

    /// <summary>Who the player sees under the fog of war (M-027, T-071.3).</summary>
    public static class FogRules
    {
        /// <summary>
        /// The player's own and allied units are always shown. Anybody else is
        /// shown while in sight; out of sight a building — anything that cannot
        /// walk, a mine too — that was seen before stays as remembered,
        /// everything else is hidden.
        /// </summary>
        public static FogSight SightOf(bool isFriendly, bool isStatic, bool isInSight, bool wasSeenBefore)
        {
            if (isFriendly || isInSight)
            {
                return FogSight.Visible;
            }

            return isStatic && wasSeenBefore ? FogSight.Remembered : FogSight.Hidden;
        }
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
    ///
    /// Some cells block sight — trees (T-071.2). A blocking cell is itself
    /// seen, what lies behind it is not. A circle with no blocker in it is
    /// filled whole, as before; only a circle with one is walked ray by ray,
    /// so open ground costs what it did.
    /// </summary>
    public sealed class FogGrid
    {
        // Rays are walked in steps of this many cells.
        private const float RayStep = 0.5f;

        private readonly FogState[] _cells;
        private readonly float[] _light;
        private readonly bool[] _blockers;

        // Blockers summed over the rectangle from the corner: is there any
        // blocker in a box, in four reads. Rebuilt when the blockers change.
        private readonly int[] _blockerSums;
        private bool _sumsDirty;

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
            _blockers = new bool[width * height];
            _blockerSums = new int[(width + 1) * (height + 1)];
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

        public bool IsBlocker(int x, int y) => _blockers[y * Width + x];

        /// <summary>Nothing blocks sight any more.</summary>
        public void ClearBlockers()
        {
            Array.Clear(_blockers, 0, _blockers.Length);
            _sumsDirty = true;
        }

        /// <summary>The cell blocks sight. Off the grid is ignored.</summary>
        public void SetBlocker(int x, int y)
        {
            if (x < 0 || y < 0 || x >= Width || y >= Height)
            {
                return;
            }

            _blockers[y * Width + x] = true;
            _sumsDirty = true;
        }

        /// <summary>
        /// Everything within the radius of the point that is not behind a
        /// blocker becomes visible. A cell counts when its centre is inside
        /// the circle. The point may lie off the grid: only the part of the
        /// circle on the grid is revealed.
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
            feather = Math.Min(feather, radius);

            if (xMin > xMax || yMin > yMax)
            {
                return;
            }

            if (HasBlockerIn(xMin, yMin, xMax, yMax))
            {
                RevealByRays(x, y, radius, feather);
                return;
            }

            for (var cy = yMin; cy <= yMax; cy++)
            {
                for (var cx = xMin; cx <= xMax; cx++)
                {
                    See(cx, cy, x, y, radius, feather);
                }
            }
        }

        /// <summary>
        /// A ray from the point to every cell on the edge of the circle's
        /// square: together they cover every cell inside. Each walks out until
        /// the radius or a blocker; the blocker is seen, the ray stops on it.
        /// A blocker under the point itself does not blind it.
        /// </summary>
        private void RevealByRays(float x, float y, float radius, float feather)
        {
            var left = (int)Math.Floor(x - radius);
            var right = (int)Math.Floor(x + radius);
            var bottom = (int)Math.Floor(y - radius);
            var top = (int)Math.Floor(y + radius);

            for (var cx = left; cx <= right; cx++)
            {
                Ray(x, y, cx, bottom, radius, feather);
                Ray(x, y, cx, top, radius, feather);
            }

            for (var cy = bottom + 1; cy < top; cy++)
            {
                Ray(x, y, left, cy, radius, feather);
                Ray(x, y, right, cy, radius, feather);
            }

            SeeForestEdge(x, y, radius, feather);
        }

        /// <summary>
        /// A ray along the edge of a forest grazes the corners of the nearer
        /// trees and stops, and the edge would go dark a few cells away. So a
        /// blocker next to seen open ground is seen too: the whole edge, never
        /// what is behind it.
        /// </summary>
        private void SeeForestEdge(float x, float y, float radius, float feather)
        {
            var xMin = Math.Max(0, (int)Math.Floor(x - radius));
            var xMax = Math.Min(Width - 1, (int)Math.Floor(x + radius));
            var yMin = Math.Max(0, (int)Math.Floor(y - radius));
            var yMax = Math.Min(Height - 1, (int)Math.Floor(y + radius));

            for (var cy = yMin; cy <= yMax; cy++)
            {
                for (var cx = xMin; cx <= xMax; cx++)
                {
                    var i = cy * Width + cx;
                    if (!_blockers[i] || _cells[i] == FogState.Visible)
                    {
                        continue;
                    }

                    if (IsSeenOpen(cx - 1, cy) || IsSeenOpen(cx + 1, cy)
                        || IsSeenOpen(cx, cy - 1) || IsSeenOpen(cx, cy + 1))
                    {
                        See(cx, cy, x, y, radius, feather);
                    }
                }
            }
        }

        private bool IsSeenOpen(int cx, int cy)
        {
            if (cx < 0 || cy < 0 || cx >= Width || cy >= Height)
            {
                return false;
            }

            var i = cy * Width + cx;
            return !_blockers[i] && _cells[i] == FogState.Visible;
        }

        private void Ray(float x, float y, int targetX, int targetY, float radius, float feather)
        {
            var dx = targetX + 0.5f - x;
            var dy = targetY + 0.5f - y;
            var length = (float)Math.Sqrt(dx * dx + dy * dy);
            if (length < 1e-4f)
            {
                return;
            }

            dx /= length;
            dy /= length;

            var startX = (int)Math.Floor(x);
            var startY = (int)Math.Floor(y);
            var end = Math.Min(length, radius + 1f);

            for (var t = 0f; t <= end; t += RayStep)
            {
                var cx = (int)Math.Floor(x + dx * t);
                var cy = (int)Math.Floor(y + dy * t);
                if (cx < 0 || cy < 0 || cx >= Width || cy >= Height)
                {
                    continue;
                }

                See(cx, cy, x, y, radius, feather);

                if (_blockers[cy * Width + cx] && (cx != startX || cy != startY))
                {
                    return;
                }
            }
        }

        /// <summary>The cell is seen from the point, if its centre is within the radius.</summary>
        private void See(int cx, int cy, float x, float y, float radius, float feather)
        {
            var dx = cx + 0.5f - x;
            var dy = cy + 0.5f - y;
            var distanceSquared = dx * dx + dy * dy;
            if (distanceSquared > radius * radius)
            {
                return;
            }

            var i = cy * Width + cx;
            _cells[i] = FogState.Visible;

            var full = radius - feather;
            var light = distanceSquared <= full * full
                ? 1f
                : (radius - (float)Math.Sqrt(distanceSquared)) / feather;
            if (light > _light[i])
            {
                _light[i] = light;
            }
        }

        private bool HasBlockerIn(int xMin, int yMin, int xMax, int yMax)
        {
            if (_sumsDirty)
            {
                RebuildSums();
            }

            var stride = Width + 1;
            var sum = _blockerSums[(yMax + 1) * stride + xMax + 1]
                - _blockerSums[yMin * stride + xMax + 1]
                - _blockerSums[(yMax + 1) * stride + xMin]
                + _blockerSums[yMin * stride + xMin];
            return sum > 0;
        }

        private void RebuildSums()
        {
            var stride = Width + 1;
            for (var y = 0; y < Height; y++)
            {
                var row = 0;
                for (var x = 0; x < Width; x++)
                {
                    row += _blockers[y * Width + x] ? 1 : 0;
                    _blockerSums[(y + 1) * stride + x + 1] = _blockerSums[y * stride + x + 1] + row;
                }
            }

            _sumsDirty = false;
        }
    }
}
