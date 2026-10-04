using Assets.Scripts.Infrastructure.Enums;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

namespace Assets.Scripts.UI
{
    /// <summary>
    /// The minimap in the top left corner (M-025): the ground of the whole map,
    /// units and buildings as dots in their team colour, a frame of what the
    /// camera sees. A click moves the camera there, dragging keeps it under the
    /// pointer. Orders are not given from here.
    ///
    /// The ground is photographed once at start from above, without units: the
    /// map does not change during a game. Dots and the frame are drawn on top
    /// a few times a second.
    ///
    /// Over the picture, everything a unit cannot walk on is darkened, taken
    /// from the NavMesh (T-057): on light ground the picture alone does not
    /// tell a rock from a field.
    ///
    /// Look of 0.3.1 (T-065.2): the photo is recoloured to dark cold slate so
    /// the team dots pop, a faint tactical grid lies over it, the map sits in a
    /// flat panel with corner brackets. Colours come from the theme.
    /// </summary>
    public sealed class Minimap : IDisposable
    {
        private const float Width = 268f;

        // One line of the tactical grid every this many metres of the map.
        private const float GridStep = 10f;
        private const long RedrawMilliseconds = 100;
        private const int GroundPixels = 512;
        private const int BlockedPixels = 256;

        private readonly VisualElement _slot;
        private readonly VisualElement _view;
        private readonly MinimapOverlay _overlay;
        private readonly CameraController _camera;
        private readonly IVisualElementScheduledItem _ticker;

        private RenderTexture _ground;
        private Texture2D _blocked;
        private int _pointerId = -1;

        public Minimap(VisualElement slot, MapValues map, CameraController camera, TeamController teams,
            UnitsController selection)
        {
            _slot = slot;
            _camera = camera;

            var a = map.LeftTopMapCornerPosition;
            var b = map.RightBottomMapCornerPosition;
            var world = Rect.MinMaxRect(Mathf.Min(a.x, b.x), Mathf.Min(a.z, b.z), Mathf.Max(a.x, b.x), Mathf.Max(a.z, b.z));

            var height = Width * world.height / Mathf.Max(1f, world.width);

            _slot.Clear();
            _slot.style.display = DisplayStyle.Flex;

            _view = new VisualElement();
            _view.AddToClassList("minimap");
            _view.style.width = Width;
            _view.style.height = height;

            _ground = RenderGround(world, camera.ControlledCamera != null ? camera.ControlledCamera : Camera.main);
            if (_ground != null)
            {
                _view.style.backgroundImage = Background.FromRenderTexture(_ground);
            }

            _blocked = RenderBlocked(world);
            if (_blocked != null)
            {
                var blocked = new VisualElement { pickingMode = PickingMode.Ignore };
                blocked.AddToClassList("minimap__blocked");
                blocked.style.backgroundImage = new StyleBackground(_blocked);
                _view.Add(blocked);
            }

            _overlay = new MinimapOverlay(world, camera, teams, selection);
            _view.Add(_overlay);

            // The panel around the map with a bracket in every corner.
            var frame = new VisualElement { pickingMode = PickingMode.Ignore };
            frame.AddToClassList("minimap-frame");
            frame.Add(_view);
            foreach (var corner in new[] { "tl", "tr", "bl", "br" })
            {
                var bracket = new VisualElement { pickingMode = PickingMode.Ignore };
                bracket.AddToClassList("minimap-frame__corner");
                bracket.AddToClassList("minimap-frame__corner--" + corner);
                frame.Add(bracket);
            }

            _slot.Add(frame);

            _view.RegisterCallback<PointerDownEvent>(OnPointerDown);
            _view.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            _view.RegisterCallback<PointerUpEvent>(OnPointerUp);

            _ticker = _view.schedule.Execute(_overlay.MarkDirtyRepaint).Every(RedrawMilliseconds);
        }

        public void Dispose()
        {
            _ticker?.Pause();

            if (_ground != null)
            {
                _ground.Release();
                UnityEngine.Object.Destroy(_ground);
                _ground = null;
            }

            if (_blocked != null)
            {
                UnityEngine.Object.Destroy(_blocked);
                _blocked = null;
            }
        }

        // --- moving the camera ----------------------------------------------------

        private void OnPointerDown(PointerDownEvent evt)
        {
            if (evt.button != 0)
            {
                // Right button and others: swallowed, the world must not get them.
                evt.StopPropagation();
                return;
            }

            _pointerId = evt.pointerId;
            _view.CapturePointer(_pointerId);
            MoveCameraTo(evt.localPosition);
            evt.StopPropagation();
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (evt.pointerId != _pointerId || !_view.HasPointerCapture(_pointerId))
            {
                return;
            }

            MoveCameraTo(evt.localPosition);
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            if (evt.pointerId != _pointerId)
            {
                return;
            }

            if (_view.HasPointerCapture(_pointerId))
            {
                _view.ReleasePointer(_pointerId);
            }

            _pointerId = -1;
        }

        private void MoveCameraTo(Vector2 local)
        {
            var point = _overlay.ToWorld(local);
            _camera.SetCamera(point);
            _overlay.MarkDirtyRepaint();
        }

        // --- the ground -------------------------------------------------------------

        /// <summary>
        /// One shot of the map from straight above, units left out (they are
        /// drawn as dots). Top of the picture is +Z, like the top of the screen.
        /// </summary>
        private static RenderTexture RenderGround(Rect world, Camera mainCamera)
        {
            var aspect = world.width / Mathf.Max(1f, world.height);
            var texture = new RenderTexture(GroundPixels, Mathf.RoundToInt(GroundPixels / aspect), 24)
            {
                name = "Minimap Ground",
                antiAliasing = 2,
            };

            var holder = new GameObject("Minimap Capture");
            try
            {
                var camera = holder.AddComponent<Camera>();
                camera.enabled = false;
                camera.orthographic = true;
                camera.orthographicSize = world.height / 2f;
                camera.aspect = aspect;
                camera.nearClipPlane = 1f;
                camera.farClipPlane = 1000f;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = mainCamera != null ? mainCamera.backgroundColor : Color.black;
                camera.cullingMask = ~(1 << (int)Layer.Unit);
                camera.transform.SetPositionAndRotation(
                    new Vector3(world.center.x, 300f, world.center.y),
                    Quaternion.Euler(90f, 0f, 0f));
                camera.targetTexture = texture;
                camera.Render();
                camera.targetTexture = null;
            }
            finally
            {
                UnityEngine.Object.Destroy(holder);
            }

            return texture;
        }

        // --- what cannot be walked on ------------------------------------------------

        /// <summary>
        /// A mask of the map: opaque where a unit cannot walk, clear where it
        /// can. Walkable is what the baked NavMesh covers. Units, buildings,
        /// trees and mines are dots or part of the picture, not walls: their
        /// own holes in the NavMesh are filled back, so a building standing at
        /// start does not leave a dark patch for the whole game. Any other
        /// obstacle is a wall and is darkened.
        /// </summary>
        private static Texture2D RenderBlocked(Rect world)
        {
            var triangulation = NavMesh.CalculateTriangulation();
            if (triangulation.indices == null || triangulation.indices.Length == 0)
            {
                return null;
            }

            var width = BlockedPixels;
            var height = Mathf.Max(1, Mathf.RoundToInt(BlockedPixels * world.height / Mathf.Max(1f, world.width)));
            var walkable = new bool[width * height];

            Vector2 ToPixel(Vector3 p) => new Vector2(
                (p.x - world.xMin) / world.width * width,
                (p.z - world.yMin) / world.height * height);

            var vertices = triangulation.vertices;
            var indices = triangulation.indices;
            for (var i = 0; i + 2 < indices.Length; i += 3)
            {
                FillTriangle(walkable, width, height,
                    ToPixel(vertices[indices[i]]), ToPixel(vertices[indices[i + 1]]), ToPixel(vertices[indices[i + 2]]));
            }

            var margin = NavMesh.GetSettingsByIndex(0).agentRadius + 0.25f;
            foreach (var obstacle in UnityEngine.Object.FindObjectsByType<NavMeshObstacle>(FindObjectsSortMode.None))
            {
                var collider = obstacle.GetComponent<Collider>();
                var bounds = collider != null ? collider.bounds : new Bounds(obstacle.transform.position, Vector3.zero);
                var isGameObject = obstacle.GetComponentInParent<UnitValues>() != null
                    || obstacle.GetComponentInParent<ResourceValues>() != null
                    || obstacle.GetComponentInParent<HarvestedResource>() != null;

                if (isGameObject)
                {
                    bounds.Expand(margin * 2f);
                }

                FillRect(walkable, width, height, ToPixel(bounds.min), ToPixel(bounds.max), isGameObject);
            }

            var pixels = new Color32[width * height];
            for (var i = 0; i < pixels.Length; i++)
            {
                pixels[i] = new Color32(255, 255, 255, walkable[i] ? (byte)0 : (byte)255);
            }

            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                name = "Minimap Blocked",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
            };
            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            return texture;
        }

        /// <summary>Marks every pixel whose centre lies in the triangle.</summary>
        private static void FillTriangle(bool[] mask, int width, int height, Vector2 a, Vector2 b, Vector2 c)
        {
            var area = Cross(b - a, c - a);
            if (Mathf.Abs(area) < 1e-6f)
            {
                return;
            }

            var xMin = Mathf.Max(0, Mathf.FloorToInt(Mathf.Min(a.x, Mathf.Min(b.x, c.x))));
            var xMax = Mathf.Min(width - 1, Mathf.CeilToInt(Mathf.Max(a.x, Mathf.Max(b.x, c.x))));
            var yMin = Mathf.Max(0, Mathf.FloorToInt(Mathf.Min(a.y, Mathf.Min(b.y, c.y))));
            var yMax = Mathf.Min(height - 1, Mathf.CeilToInt(Mathf.Max(a.y, Mathf.Max(b.y, c.y))));

            // A little slack, so pixels on an edge shared by two triangles are not lost.
            var slack = -0.02f * Mathf.Abs(area);
            var sign = Mathf.Sign(area);

            for (var y = yMin; y <= yMax; y++)
            {
                for (var x = xMin; x <= xMax; x++)
                {
                    var p = new Vector2(x + 0.5f, y + 0.5f);
                    if (Cross(b - a, p - a) * sign >= slack
                        && Cross(c - b, p - b) * sign >= slack
                        && Cross(a - c, p - c) * sign >= slack)
                    {
                        mask[y * width + x] = true;
                    }
                }
            }
        }

        private static void FillRect(bool[] mask, int width, int height, Vector2 min, Vector2 max, bool value)
        {
            var xMin = Mathf.Max(0, Mathf.FloorToInt(min.x));
            var xMax = Mathf.Min(width - 1, Mathf.CeilToInt(max.x) - 1);
            var yMin = Mathf.Max(0, Mathf.FloorToInt(min.y));
            var yMax = Mathf.Min(height - 1, Mathf.CeilToInt(max.y) - 1);

            for (var y = yMin; y <= yMax; y++)
            {
                for (var x = xMin; x <= xMax; x++)
                {
                    mask[y * width + x] = value;
                }
            }
        }

        private static float Cross(Vector2 u, Vector2 v) => u.x * v.y - u.y * v.x;

        /// <summary>Dots and the camera frame, drawn over the ground picture.</summary>
        private sealed class MinimapOverlay : VisualElement
        {
            private readonly Rect _world;
            private readonly CameraController _camera;
            private readonly TeamController _teams;
            private readonly UnitsController _selection;
            private readonly Dictionary<int, Color> _teamColors = new Dictionary<int, Color>();
            private readonly HashSet<GameObject> _selected = new HashSet<GameObject>();
            private readonly Vector3[] _frameCorners = new Vector3[4];

            private static readonly CustomStyleProperty<Color> GridProperty = new CustomStyleProperty<Color>("--minimap-grid");
            private static readonly CustomStyleProperty<Color> ViewProperty = new CustomStyleProperty<Color>("--minimap-view");
            private static readonly CustomStyleProperty<Color> ViewFillProperty = new CustomStyleProperty<Color>("--minimap-view-fill");
            private static readonly CustomStyleProperty<Color> DotEdgeProperty = new CustomStyleProperty<Color>("--minimap-dot-edge");
            private static readonly CustomStyleProperty<Color> SelectedProperty = new CustomStyleProperty<Color>("--minimap-selected");

            private Color _grid = new Color(1f, 1f, 1f, 0.08f);
            private Color _view = Color.white;
            private Color _viewFill = new Color(1f, 1f, 1f, 0.06f);
            private Color _dotEdge = new Color(0f, 0f, 0f, 0.85f);
            private Color _selectedEdge = Color.white;

            public MinimapOverlay(Rect world, CameraController camera, TeamController teams, UnitsController selection)
            {
                _world = world;
                _camera = camera;
                _teams = teams;
                _selection = selection;

                AddToClassList("minimap__overlay");
                pickingMode = PickingMode.Ignore;
                generateVisualContent += Draw;
                RegisterCallback<CustomStyleResolvedEvent>(OnStyleResolved);
            }

            private void OnStyleResolved(CustomStyleResolvedEvent evt)
            {
                if (evt.customStyle.TryGetValue(GridProperty, out var grid)) _grid = grid;
                if (evt.customStyle.TryGetValue(ViewProperty, out var view)) _view = view;
                if (evt.customStyle.TryGetValue(ViewFillProperty, out var viewFill)) _viewFill = viewFill;
                if (evt.customStyle.TryGetValue(DotEdgeProperty, out var dotEdge)) _dotEdge = dotEdge;
                if (evt.customStyle.TryGetValue(SelectedProperty, out var selected)) _selectedEdge = selected;
                MarkDirtyRepaint();
            }

            public Vector3 ToWorld(Vector2 local)
            {
                var size = contentRect.size;
                var u = Mathf.Clamp01(local.x / Mathf.Max(1f, size.x));
                var v = Mathf.Clamp01(local.y / Mathf.Max(1f, size.y));
                return new Vector3(_world.xMin + u * _world.width, 0f, _world.yMax - v * _world.height);
            }

            private Vector2 ToLocal(Vector3 world)
            {
                var size = contentRect.size;
                var u = (world.x - _world.xMin) / _world.width;
                var v = (_world.yMax - world.z) / _world.height;
                return new Vector2(u * size.x, v * size.y);
            }

            private Color ColorOf(int teamId)
            {
                if (_teamColors.TryGetValue(teamId, out var color))
                {
                    return color;
                }

                color = Color.gray;
                if (_teams != null && _teams.Teams != null)
                {
                    foreach (var team in _teams.Teams)
                    {
                        if (team.Id == teamId)
                        {
                            color = team.Color;
                            color.a = 1f;
                            break;
                        }
                    }
                }

                _teamColors[teamId] = color;
                return color;
            }

            private void Draw(MeshGenerationContext context)
            {
                var painter = context.painter2D;

                _selected.Clear();
                if (_selection != null)
                {
                    foreach (var unit in _selection.SelectedUnits)
                    {
                        if (unit != null) _selected.Add(unit);
                    }
                }

                var outline = _dotEdge;

                DrawGrid(painter);

                // Buildings first, so that units standing by them stay visible.
                for (var pass = 0; pass < 2; pass++)
                {
                    foreach (var record in UnitRegistry.All)
                    {
                        if (record.GameObject == null || record.Values == null)
                        {
                            continue;
                        }

                        var isBuilding = record.Values.IsBuilding;
                        if ((pass == 0) != isBuilding)
                        {
                            continue;
                        }

                        var at = ToLocal(record.Transform.position);
                        var color = ColorOf(record.TeamId);
                        var selected = _selected.Contains(record.GameObject);

                        if (isBuilding)
                        {
                            var half = Mathf.Clamp(record.Size * 1.3f, 3f, 7f);
                            var rect = new Rect(at.x - half, at.y - half, half * 2f, half * 2f);
                            FillRect(painter, rect, selected ? _selectedEdge : outline, 1.5f);
                            FillRect(painter, rect, color, 0f);
                        }
                        else
                        {
                            FillCircle(painter, at, selected ? 4.2f : 3.6f, selected ? _selectedEdge : outline);
                            FillCircle(painter, at, 2.6f, color);
                        }
                    }
                }

                DrawCameraFrame(painter);
            }

            private void DrawCameraFrame(Painter2D painter)
            {
                // Taken every time: the controller may find its camera after the HUD is built.
                var camera = _camera != null && _camera.ControlledCamera != null ? _camera.ControlledCamera : Camera.main;
                if (camera == null)
                {
                    return;
                }

                var ground = new Plane(Vector3.up, Vector3.zero);
                var screen = new[]
                {
                    new Vector3(0f, 0f), new Vector3(Screen.width, 0f),
                    new Vector3(Screen.width, Screen.height), new Vector3(0f, Screen.height),
                };

                for (var i = 0; i < 4; i++)
                {
                    var ray = camera.ScreenPointToRay(screen[i]);
                    if (!ground.Raycast(ray, out var distance))
                    {
                        return;
                    }

                    _frameCorners[i] = ray.GetPoint(distance);
                }

                // A light veil over what the camera sees, then dark under light,
                // so the frame reads on any ground.
                painter.fillColor = _viewFill;
                painter.BeginPath();
                painter.MoveTo(ToLocal(_frameCorners[0]));
                for (var i = 1; i < 4; i++)
                {
                    painter.LineTo(ToLocal(_frameCorners[i]));
                }
                painter.ClosePath();
                painter.Fill();

                StrokeFrame(painter, 3.5f, _dotEdge);
                StrokeFrame(painter, 1.5f, _view);
            }

            /// <summary>Thin lines every GridStep metres, from the map's own corner.</summary>
            private void DrawGrid(Painter2D painter)
            {
                var size = contentRect.size;
                if (size.x <= 0f || size.y <= 0f)
                {
                    return;
                }

                painter.lineWidth = 1f;
                painter.strokeColor = _grid;
                painter.BeginPath();

                for (var x = Mathf.Ceil(_world.xMin / GridStep) * GridStep; x <= _world.xMax; x += GridStep)
                {
                    var u = (x - _world.xMin) / _world.width * size.x;
                    painter.MoveTo(new Vector2(u, 0f));
                    painter.LineTo(new Vector2(u, size.y));
                }

                for (var z = Mathf.Ceil(_world.yMin / GridStep) * GridStep; z <= _world.yMax; z += GridStep)
                {
                    var v = (_world.yMax - z) / _world.height * size.y;
                    painter.MoveTo(new Vector2(0f, v));
                    painter.LineTo(new Vector2(size.x, v));
                }

                painter.Stroke();
            }

            private void StrokeFrame(Painter2D painter, float width, Color color)
            {
                painter.lineJoin = LineJoin.Round;
                painter.lineWidth = width;
                painter.strokeColor = color;
                painter.BeginPath();
                painter.MoveTo(ToLocal(_frameCorners[0]));
                for (var i = 1; i < 4; i++)
                {
                    painter.LineTo(ToLocal(_frameCorners[i]));
                }
                painter.ClosePath();
                painter.Stroke();
            }

            private static void FillCircle(Painter2D painter, Vector2 center, float radius, Color color)
            {
                painter.fillColor = color;
                painter.BeginPath();
                painter.Arc(center, radius, Angle.Degrees(0f), Angle.Degrees(360f));
                painter.Fill();
            }

            private static void FillRect(Painter2D painter, Rect rect, Color color, float grow)
            {
                rect.xMin -= grow;
                rect.yMin -= grow;
                rect.xMax += grow;
                rect.yMax += grow;

                painter.fillColor = color;
                painter.BeginPath();
                painter.MoveTo(new Vector2(rect.xMin, rect.yMin));
                painter.LineTo(new Vector2(rect.xMax, rect.yMin));
                painter.LineTo(new Vector2(rect.xMax, rect.yMax));
                painter.LineTo(new Vector2(rect.xMin, rect.yMax));
                painter.ClosePath();
                painter.Fill();
            }
        }
    }
}
