using Assets.Scripts.Infrastructure.Enums;
using System;

namespace Assets.Scripts.Infrastructure.Events
{
    public delegate void ResourceChangedHandler(ResourceChangedEventArgs args);

    public class ResourceChangedEventArgs : EventArgs
    {
        public ResourceChangedEventArgs(ResourceDefinition name, ResourceType type, int oldValue, int newValue)
        {
            Name = name;
            Type = type;
            OldValue = oldValue;
            NewValue = newValue;
        }

        public ResourceDefinition Name { get; set; }

        public ResourceType Type { get; set; }

        public int OldValue { get; set; }

        public int NewValue { get; set; }
    }
}
