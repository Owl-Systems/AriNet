using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace AriNetClient.Dash.Events
{
    public interface IEventSubscriber
    {
        string Id { get; }
        Task HandleEventAsync(WazoEvent @event);
    }

    public interface IEventAggregator
    {
        void Subscribe<TEvent>(Func<TEvent, Task> handler) where TEvent : WazoEvent;
        void Subscribe(string eventType, Func<WazoEvent, Task> handler);
        void Unsubscribe<TEvent>(Func<TEvent, Task> handler) where TEvent : WazoEvent;
        Task PublishAsync(WazoEvent @event);
    }

    public class EventAggregator : IEventAggregator
    {
        private readonly ConcurrentDictionary<Type, List<Func<WazoEvent, Task>>> _typedHandlers;
        private readonly ConcurrentDictionary<string, List<Func<WazoEvent, Task>>> _eventTypeHandlers;
        private readonly ILogger<EventAggregator> _logger;

        public EventAggregator(ILogger<EventAggregator> logger)
        {
            _typedHandlers = new ConcurrentDictionary<Type, List<Func<WazoEvent, Task>>>();
            _eventTypeHandlers = new ConcurrentDictionary<string, List<Func<WazoEvent, Task>>>();
            _logger = logger;
        }

        public void Subscribe<TEvent>(Func<TEvent, Task> handler) where TEvent : WazoEvent
        {
            var eventType = typeof(TEvent);
            var handlers = _typedHandlers.GetOrAdd(eventType, _ => new List<Func<WazoEvent, Task>>());

            lock (handlers)
            {
                handlers.Add(e => handler((TEvent)e));
            }

            _logger?.LogDebug("Subscribed typed handler for {EventType}", eventType.Name);
        }

        public void Subscribe(string eventType, Func<WazoEvent, Task> handler)
        {
            var handlers = _eventTypeHandlers.GetOrAdd(eventType, _ => new List<Func<WazoEvent, Task>>());

            lock (handlers)
            {
                handlers.Add(handler);
            }

            _logger?.LogDebug("Subscribed event type handler for {EventType}", eventType);
        }

        public void Unsubscribe<TEvent>(Func<TEvent, Task> handler) where TEvent : WazoEvent
        {
            var eventType = typeof(TEvent);
            if (_typedHandlers.TryGetValue(eventType, out var handlers))
            {
                lock (handlers)
                {
                    handlers.Remove(e => handler((TEvent)e));
                }
            }
        }

        public async Task PublishAsync(WazoEvent @event)
        {
            var tasks = new List<Task>();

            // نشر إلى معالجي النوع المحدد
            if (_typedHandlers.TryGetValue(@event.GetType(), out var typedHandlers))
            {
                foreach (var handler in typedHandlers.ToArray())
                {
                    tasks.Add(SafeExecuteHandler(handler, @event));
                }
            }

            // نشر إلى معالجي نوع الحدث (string)
            if (_eventTypeHandlers.TryGetValue(@event.EventType, out var eventTypeHandlers))
            {
                foreach (var handler in eventTypeHandlers.ToArray())
                {
                    tasks.Add(SafeExecuteHandler(handler, @event));
                }
            }

            await Task.WhenAll(tasks);
        }

        private async Task SafeExecuteHandler(Func<WazoEvent, Task> handler, WazoEvent @event)
        {
            try
            {
                await handler(@event);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error executing event handler for {EventType}", @event.EventType);
            }
        }
    }
}
