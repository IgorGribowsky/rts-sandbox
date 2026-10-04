using Assets.Scripts.Infrastructure.Enums;
using Assets.Scripts.Infrastructure.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts.UI
{
    /// <summary>
    /// "+10" with the resource icon over the place the income came from: a
    /// storage that took wood, a mine that gave gold (M-022). It pops, rises
    /// and fades.
    ///
    /// Crits use the same marks (T-070): red "18!" over the attacker, no icon,
    /// rising slower and living longer. They are never merged — every crit is
    /// a blow of its own.
    ///
    /// The marks are a fixed pool: twenty workers handing in at once reuse the
    /// oldest mark instead of growing the tree. Income of the same resource at
    /// the same place within a moment is added to the mark already there, so
    /// the numbers do not pile into a mess.
    ///
    /// Runs every frame only while some mark is alive.
    /// </summary>
    public sealed class ResourcePopups : IDisposable
    {
        private const int PoolSize = 24;
        private const float Lifetime = 1.3f;
        private const float RisePixels = 70f;
        private const float PopSeconds = 0.14f;
        private const float FadeFrom = 0.55f;
        private const float MergeSeconds = 0.35f;
        private const float MergeDistance = 1.5f;
        private const float CritLifetime = 1.7f;
        private const float CritRisePixels = 55f;

        private readonly VisualElement _layer;
        private readonly PlayerEventController _events;
        private readonly Dictionary<ResourceName, Texture2D> _icons = new Dictionary<ResourceName, Texture2D>();
        private readonly List<Mark> _pool = new List<Mark>();
        private readonly IVisualElementScheduledItem _tick;

        private Camera _camera;

        public ResourcePopups(VisualElement layer, GameResources gameResources, PlayerEventController events)
        {
            _layer = layer;
            _events = events;
            _layer.Clear();

            foreach (var resource in gameResources.Resources)
            {
                _icons[resource.ResourceName] = resource.Icon;
            }

            for (var i = 0; i < PoolSize; i++)
            {
                var mark = new Mark();
                _layer.Add(mark.Root);
                _pool.Add(mark);
            }

            _tick = _layer.schedule.Execute(Update).Every(0);
            _tick.Pause();

            _events.ResourceGained += OnResourceGained;
            _events.CriticalHit += OnCriticalHit;
        }

        public void Dispose()
        {
            _events.ResourceGained -= OnResourceGained;
            _events.CriticalHit -= OnCriticalHit;
            _tick.Pause();
            _layer.Clear();
            _pool.Clear();
        }

        private void OnResourceGained(ResourceGainedEventArgs args)
        {
            if (args.Amount <= 0)
            {
                return;
            }

            var now = Time.time;

            var same = _pool.FirstOrDefault(m => m.Alive
                && !m.IsCrit
                && m.Resource == args.Name
                && now - m.Born < MergeSeconds
                && (m.World - args.Position).sqrMagnitude < MergeDistance * MergeDistance);
            if (same != null)
            {
                same.Amount += args.Amount;
                same.Born = now;
                same.Refresh();
                return;
            }

            // A free mark, or the oldest one when all are busy.
            var mark = _pool.FirstOrDefault(m => !m.Alive) ?? _pool.OrderBy(m => m.Born).First();

            mark.Alive = true;
            mark.SetCrit(false);
            mark.Resource = args.Name;
            mark.Amount = args.Amount;
            mark.World = args.Position;
            mark.Born = now;
            mark.SetIcon(_icons.TryGetValue(args.Name, out var icon) ? icon : null);
            mark.Refresh();

            // The newest on top of the older ones.
            mark.Root.BringToFront();

            _tick.Resume();
            Place(mark, now);
        }

        private void OnCriticalHit(CriticalHitEventArgs args)
        {
            var now = Time.time;
            var mark = _pool.FirstOrDefault(m => !m.Alive) ?? _pool.OrderBy(m => m.Born).First();

            mark.Alive = true;
            mark.SetCrit(true);
            mark.Amount = Mathf.RoundToInt(args.Damage);
            mark.World = args.Position;
            mark.Born = now;
            mark.SetIcon(null);
            mark.Refresh();
            mark.Root.BringToFront();

            _tick.Resume();
            Place(mark, now);
        }

        private void Update()
        {
            var now = Time.time;
            var anyAlive = false;

            foreach (var mark in _pool)
            {
                if (!mark.Alive)
                {
                    continue;
                }

                if (now - mark.Born >= mark.Lifetime)
                {
                    mark.Hide();
                    continue;
                }

                anyAlive = true;
                Place(mark, now);
            }

            if (!anyAlive)
            {
                _tick.Pause();
            }
        }

        private void Place(Mark mark, float now)
        {
            if (_camera == null)
            {
                _camera = Camera.main;
            }

            var panel = _layer.panel;
            if (_camera == null || panel == null)
            {
                mark.Root.style.display = DisplayStyle.None;
                return;
            }

            // Behind the camera there is nothing to show; off the screen the
            // mark simply lies outside the panel, it is never pulled to the edge.
            var view = _camera.WorldToViewportPoint(mark.World);
            if (view.z <= 0f)
            {
                mark.Root.style.display = DisplayStyle.None;
                return;
            }

            var age = now - mark.Born;
            var t = age / mark.Lifetime;

            var point = RuntimePanelUtils.CameraTransformWorldToPanel(panel, mark.World, _camera);

            // A crit rises evenly and slowly, the income shoots up and settles.
            var rise = mark.IsCrit
                ? CritRisePixels * t
                : RisePixels * (1f - (1f - t) * (1f - t));

            mark.Root.style.display = DisplayStyle.Flex;
            mark.Root.style.translate = new Translate(point.x, point.y - rise);

            var pop = age < PopSeconds ? Mathf.Lerp(1.35f, 1f, age / PopSeconds) : 1f;
            mark.Body.style.scale = new Scale(new Vector3(pop, pop, 1f));
            mark.Root.style.opacity = t < FadeFrom ? 1f : 1f - (t - FadeFrom) / (1f - FadeFrom);
        }

        /// <summary>One "+N" with its icon.</summary>
        private sealed class Mark
        {
            public readonly VisualElement Root;
            public readonly VisualElement Body;
            private readonly VisualElement _icon;
            private readonly Label _label;

            public bool Alive;
            public bool IsCrit;
            public ResourceName Resource;
            public int Amount;
            public Vector3 World;
            public float Born;

            public Mark()
            {
                Root = new VisualElement { pickingMode = PickingMode.Ignore };
                Root.AddToClassList("popup");

                Body = new VisualElement { pickingMode = PickingMode.Ignore };
                Body.AddToClassList("popup__body");
                Root.Add(Body);
                var body = Body;

                _label = new Label { pickingMode = PickingMode.Ignore };
                _label.AddToClassList("popup__value");
                _label.AddToClassList("hud-text");
                body.Add(_label);

                _icon = new VisualElement { pickingMode = PickingMode.Ignore };
                _icon.AddToClassList("popup__icon");
                body.Add(_icon);

                Hide();
            }

            public float Lifetime => IsCrit ? CritLifetime : ResourcePopups.Lifetime;

            public void SetCrit(bool crit)
            {
                IsCrit = crit;
                Root.EnableInClassList("popup--crit", crit);
            }

            public void SetIcon(Texture2D icon)
            {
                _icon.style.backgroundImage = icon != null ? new StyleBackground(icon) : StyleKeyword.None;
            }

            public void Refresh()
            {
                _label.text = IsCrit ? UiText.Critical(Amount) : UiText.Income(Amount);
            }

            public void Hide()
            {
                Alive = false;
                Root.style.display = DisplayStyle.None;
            }
        }
    }
}
