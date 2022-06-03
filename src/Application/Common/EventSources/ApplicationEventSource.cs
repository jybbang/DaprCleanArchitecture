﻿using MediatR;
using Microsoft.Extensions.Logging;
using NetCoreCleanArchitecture.Domain.Common;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace NetCoreCleanArchitecture.Application.Common.EventSources
{
    public class ApplicationEventSource : IApplicationEventSource
    {
        private readonly ILoggerFactory _logFactory;
        private readonly IEventBus _eventBus;
        private readonly IPublisher _mediator;
        private readonly EventBufferService _eventBuffer;

        public ApplicationEventSource(
            ILoggerFactory logFactory,
            IEventBus eventBus,
            IPublisher mediator,
            EventBufferService eventBuffer)
        {
            _logFactory = logFactory;
            _eventBus = eventBus;
            _mediator = mediator;
            _eventBuffer = eventBuffer;
        }

        public async Task PublishAsync<TDomainEvent>(TDomainEvent domainEvent, DateTimeOffset timestamp, CancellationToken cancellationToken) where TDomainEvent : DomainEvent
        {
            var eventName = domainEvent.GetType().Name;

            var logger = _logFactory.CreateLogger(eventName);

            var timer = new Stopwatch();

            try
            {
                timer.Start();

                logger.LogTrace("Publishing Event {Name} - {@Event}", eventName, domainEvent);

                domainEvent.Publising(timestamp);

                await PublishEventNotification(domainEvent, cancellationToken);

                await PublishToEventbus(domainEvent, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Publishing Event Unhandled Exception {Name} - {@Event}", eventName, domainEvent);
            }
            finally
            {
                timer.Stop();

                var elapsedMilliseconds = timer.ElapsedMilliseconds;

                if (elapsedMilliseconds > 1000)
                {
                    logger.LogWarning("Publishing Event Long Running {Name} ({ElapsedMilliseconds} milliseconds) - {@Event}",
                        eventName, elapsedMilliseconds, domainEvent);
                }
            }
        }

        private Task PublishEventNotification<TDomainEvent>(TDomainEvent domainEvent, CancellationToken cancellationToken) where TDomainEvent : DomainEvent
        {
            var notification = (INotification)Activator.CreateInstance(
                typeof(DomainEventNotification<>).MakeGenericType(domainEvent.GetType()), domainEvent);

            return _mediator.Publish(notification, cancellationToken);
        }

        private async Task PublishToEventbus<TDomainEvent>(TDomainEvent domainEvent, CancellationToken cancellationToken) where TDomainEvent : DomainEvent
        {
            if (domainEvent is BufferedDomainEvent bufferedEvent)
            {
                _eventBuffer.BufferPublish(domainEvent.Topic, bufferedEvent);

                return;
            }

            await _eventBus.PublishAsync(domainEvent.Topic, domainEvent, cancellationToken);
        }
    }
}
