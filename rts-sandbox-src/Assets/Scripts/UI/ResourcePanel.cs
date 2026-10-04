using Assets.Scripts.Infrastructure.Enums;
using Assets.Scripts.Infrastructure.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace Assets.Scripts.UI
{
    /// <summary>
    /// The player's resources across the top of the screen (M-022). The list
    /// comes from GameResources, so a resource asset added there shows up here
    /// with no change to the interface (T-052). A supply resource reads "used/limit".
    ///
    /// Redrawn on ResourceChanged only, never polled.
    /// </summary>
    public sealed class ResourcePanel : IDisposable
    {
        private const long BumpMilliseconds = 260;

        private readonly GameResources _gameResources;
        private readonly PlayerResources _playerResources;
        private readonly PlayerEventController _events;

        private readonly Dictionary<ResourceDefinition, Label> _values = new Dictionary<ResourceDefinition, Label>();

        public ResourcePanel(VisualElement bar, GameResources gameResources,
            PlayerResources playerResources, PlayerEventController events)
        {
            _gameResources = gameResources;
            _playerResources = playerResources;
            _events = events;

            bar.Clear();

            foreach (var resource in _gameResources.Definitions)
            {
                var item = new VisualElement();
                item.AddToClassList("resource-item");
                item.pickingMode = PickingMode.Ignore;

                var icon = new VisualElement();
                icon.AddToClassList("resource-icon");
                icon.pickingMode = PickingMode.Ignore;
                if (resource.Icon != null)
                {
                    icon.style.backgroundImage = new StyleBackground(resource.Icon);
                }

                var value = new Label();
                value.AddToClassList("resource-value");
                value.AddToClassList("hud-text");
                value.pickingMode = PickingMode.Ignore;

                item.Add(icon);
                item.Add(value);
                bar.Add(item);

                _values[resource] = value;
                Refresh(resource);
            }

            _events.ResourceChanged += OnResourceChanged;
        }

        public void Dispose()
        {
            _events.ResourceChanged -= OnResourceChanged;
        }

        private void OnResourceChanged(ResourceChangedEventArgs args)
        {
            var resource = args.Name;
            if (resource == null || !_values.ContainsKey(resource))
            {
                return;
            }

            Refresh(resource);

            // A short flash when the amount goes up: income is noticed without
            // staring at the number.
            if (args.NewValue > args.OldValue && resource.Type != ResourceType.SupplyResource
                && _values.TryGetValue(resource, out var label))
            {
                label.AddToClassList("resource-value--bump");
                label.schedule.Execute(() => label.RemoveFromClassList("resource-value--bump"))
                    .StartingIn(BumpMilliseconds);
            }
        }

        private void Refresh(ResourceDefinition resource)
        {
            if (resource == null || !_values.TryGetValue(resource, out var label))
            {
                return;
            }

            var amount = Amount(_playerResources.ResourcesAmount, resource);

            label.text = resource.Type == ResourceType.SupplyResource
                ? amount + "/" + Amount(_playerResources.MaxSupplyResourcesAmount, resource)
                : amount.ToString();
        }

        private static int Amount(List<ResourceAmount> list, ResourceDefinition name)
        {
            var entry = list.FirstOrDefault(x => x.Resource == name);
            return entry != null ? entry.Amount : 0;
        }
    }
}
