﻿using NetCoreCleanArchitecture.Domain.Common;
using System.Runtime.CompilerServices;

namespace NetCoreCleanArchitecture.Domain.Events
{
    public sealed class PropertyChangedEvent<T> : DomainEvent
    {
        public PropertyChangedEvent(Entity source, long version, T oldState, T newState, [CallerMemberName] string propertyName = default)
            : base(source, version, $"{source.GetType().Name}{propertyName}PropertyChangedEvent")
        {
            PropertyName = propertyName;
            OldState = oldState;
            NewState = newState;
        }

        public string PropertyName { get; }

        public T OldState { get; }

        public T NewState { get; }
    }
}
