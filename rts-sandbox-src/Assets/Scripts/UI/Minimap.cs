using Assets.Scripts.Infrastructure.Enums;
using System;
using System.Collections.Generic;
using UnityEngine;
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
    /// </summary>
    public sealed class Minimap : IDisposable
    {
        private const float Width = 260f;
        private const long RedrawMilliseconds = 100;
        private const int GroundPixels = 512;

        private readonly VisualElement _slot;
        private readonly VisualElement _view;
        private readonly MinimapOverlay _overlay;
        private readonly CameraController _camera;
        private readonly IVisualElementScheduledItem _ticker;

        private RenderTexture _ground;
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

            _overlay = new MinimapOverlay(world, camera, teams, selection);
            _view.Add(_overlay);
            _slot.Add(_view);

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

            public MinimapOverlay(Rect world, CameraController camera, TeamController teams, UnitsController selection)
            {
                _world = world;
                _camera = camera;
                _teams = teams;
                _selection = selection;

                AddToClassList("minimap__overlay");
                pickingMode = PickingMode.Ignore;
                generateVisualContent += Draw;
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

                var outline = new Color(0f, 0f, 0f, 0.85f);

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
                            FillRect(painter, rect, selected ? Color.white : outline, 1.5f);
                            FillRect(painter, rect, color, 0f);
                        }
                        else
                        {
                            FillCircle(painter, at, selected ? 4.2f : 3.6f, selected ? Color.white : outline);
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

                // Dark under light, so the frame reads on any ground.
                StrokeFrame(painter, 4f, new Color(0f, 0f, 0f, 0.7f));
                StrokeFrame(painter, 2f, Color.white);
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
