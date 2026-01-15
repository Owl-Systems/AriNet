using AriNetClient.WebSockets.Abstracts;
using AriNetClient.WebSockets.Events;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;

namespace AriNetClient.WebSockets.Clients.EventHandling
{
    /// <summary>
    /// سجل معالجات الأحداث (تنفيذ IEventHandlerRegistry)
    /// </summary>
    public class EventHandlerRegistry : IEventHandlerRegistry
    {
        private readonly ConcurrentDictionary<Type, List<object>> _handlers = new();
        private readonly List<IGlobalEventHandler> _globalHandlers = new();
        private readonly object _lock = new();

        public void RegisterHandler<TEvent, THandler>() where TEvent : BaseEvent where THandler : IEventHandler<TEvent>
        {
            var eventType = typeof(TEvent);
            var handlerType = typeof(THandler);

            lock (_lock)
            {
                if (!_handlers.ContainsKey(eventType))
                {
                    _handlers[eventType] = new List<object>();
                }

                // التحقق من عدم تسجيل المعالج مسبقاً
                var existingHandlers = _handlers[eventType];
                if (!existingHandlers.Any(h => h.GetType() == handlerType))
                {
                    // إنشاء مثيل جديد من المعالج
                    var handler = Activator.CreateInstance<THandler>();
                    existingHandlers.Add(handler);

                    // ترتيب المعالجات حسب ExecutionOrder
                    existingHandlers.Sort((a, b) =>
                    {
                        var orderA = ((IEventHandler<TEvent>)a).ExecutionOrder;
                        var orderB = ((IEventHandler<TEvent>)b).ExecutionOrder;
                        return orderA.CompareTo(orderB);
                    });
                }
            }
        }

        public void UnregisterHandler<TEvent, THandler>() where TEvent : BaseEvent where THandler : IEventHandler<TEvent>
        {
            var eventType = typeof(TEvent);
            var handlerType = typeof(THandler);

            lock (_lock)
            {
                if (_handlers.TryGetValue(eventType, out var handlers))
                {
                    var handlerToRemove = handlers.FirstOrDefault(h => h.GetType() == handlerType);
                    if (handlerToRemove != null)
                    {
                        handlers.Remove(handlerToRemove);

                        if (handlers.Count == 0)
                        {
                            _handlers.TryRemove(eventType, out _);
                        }
                    }
                }
            }
        }

        public ReadOnlyCollection<IEventHandler<TEvent>> GetHandlers<TEvent>() where TEvent : BaseEvent
        {
            var eventType = typeof(TEvent);

            if (_handlers.TryGetValue(eventType, out var handlers))
            {
                return handlers
                    .Cast<IEventHandler<TEvent>>()
                    .ToList()
                    .AsReadOnly();
            }

            return new List<IEventHandler<TEvent>>().AsReadOnly();
        }

        public ReadOnlyCollection<IGlobalEventHandler> GetGlobalHandlers()
        {
            lock (_lock)
            {
                return _globalHandlers.AsReadOnly();
            }
        }

        public void RegisterGlobalHandler(IGlobalEventHandler handler)
        {
            lock (_lock)
            {
                if (!_globalHandlers.Contains(handler))
                {
                    _globalHandlers.Add(handler);

                    // ترتيب المعالجات العامة حسب ExecutionOrder
                    _globalHandlers.Sort((a, b) => a.ExecutionOrder.CompareTo(b.ExecutionOrder));
                }
            }
        }

        public void UnregisterGlobalHandler(IGlobalEventHandler handler)
        {
            lock (_lock)
            {
                _globalHandlers.Remove(handler);
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                _handlers.Clear();
                _globalHandlers.Clear();
            }
        }

        public bool HasHandlers<TEvent>() where TEvent : BaseEvent
        {
            var eventType = typeof(TEvent);
            return _handlers.ContainsKey(eventType) && _handlers[eventType].Count > 0;
        }

        public int GetTotalHandlerCount()
        {
            lock (_lock)
            {
                int count = _globalHandlers.Count;
                foreach (var handlers in _handlers.Values)
                {
                    count += handlers.Count;
                }
                return count;
            }
        }
    }
}
