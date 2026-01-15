using AriNetClient.WebSocket.Services.EventHandlers;
using AriNetClient.WebSocket.Statistics;
using AriNetClient.WebSockets.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Reflection;

namespace AriNetClient.WebSocket.Clients.EventHandling
{
    /// <summary>
    /// سجل معالجات الأفات الذكي - النسخة المحسنة والمستبدلة
    /// </summary>
    public class AutoEventHandlerRegistry : IEventHandlerRegistry
    {
        private readonly ConcurrentDictionary<Type, List<object>> _typedHandlers = new();
        private readonly List<IGlobalEventHandler> _globalHandlers = new();
        private readonly List<IAutoEventHandler> _autoHandlers = new();
        private readonly object _syncLock = new();
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AutoEventHandlerRegistry> _logger;

        public AutoEventHandlerRegistry(
            IServiceProvider serviceProvider,
            ILogger<AutoEventHandlerRegistry> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _logger.LogDebug("AutoEventHandlerRegistry initialized");
        }

        /// <summary>
        /// تسجيل معالج تلقائياً (الطريقة الرئيسية)
        /// </summary>
        public void RegisterHandler<THandler>() where THandler : class
        {
            RegisterHandler(typeof(THandler));
        }

        /// <summary>
        /// تسجيل معالج بواسطة النوع
        /// </summary>
        public void RegisterHandler(Type handlerType)
        {
            if (handlerType == null)
                throw new ArgumentNullException(nameof(handlerType));

            if (handlerType.IsAbstract || handlerType.IsInterface)
                throw new ArgumentException($"Cannot register abstract or interface type: {handlerType.Name}");

            lock (_syncLock)
            {
                try
                {
                    _logger.LogDebug("Registering handler type: {HandlerType}", handlerType.Name);

                    // محاولة إنشاء مثيل باستخدام DI
                    var handlerInstance = CreateHandlerInstance(handlerType);

                    if (handlerInstance == null)
                    {
                        _logger.LogWarning("Failed to create instance of handler: {HandlerType}", handlerType.Name);
                        return;
                    }

                    // تحديد نوع المعالج وتسجيله
                    RegisterHandlerInstance(handlerInstance);

                    _logger.LogInformation("Successfully registered handler: {HandlerType}", handlerType.Name);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to register handler type: {HandlerType}", handlerType.Name);
                    throw;
                }
            }
        }

        /// <summary>
        /// إنشاء مثيل المعالج باستخدام DI
        /// </summary>
        private object CreateHandlerInstance(Type handlerType)
        {
            try
            {
                // المحاولة مع DI أولاً
                return ActivatorUtilities.CreateInstance(_serviceProvider, handlerType);
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Failed to create handler with DI, trying without: {HandlerType}", handlerType.Name);

                // المحاولة بدون DI
                try
                {
                    return Activator.CreateInstance(handlerType);
                }
                catch (Exception innerEx)
                {
                    _logger.LogError(innerEx, "Failed to create handler without DI: {HandlerType}", handlerType.Name);
                    throw;
                }
            }
        }

        /// <summary>
        /// تسجيل مثيل المعالج بعد اكتشاف نوعه
        /// </summary>
        private void RegisterHandlerInstance(object handlerInstance)
        {
            // 1. التحقق إذا كان معالجاً عاماً
            if (handlerInstance is IGlobalEventHandler globalHandler)
            {
                RegisterGlobalHandlerInternal(globalHandler);
                return;
            }

            // 2. التحقق إذا كان معالجاً ذكياً (IAutoEventHandler)
            if (handlerInstance is IAutoEventHandler autoEventHandler)
            {
                RegisterAutoHandlerInternal(autoEventHandler);
                return;
            }

            // 3. اكتشاف تلقائي لأنواع الأحداث
            DiscoverAndRegisterEventTypes(handlerInstance);
        }

        /// <summary>
        /// تسجيل معالج عام
        /// </summary>
        private void RegisterGlobalHandlerInternal(IGlobalEventHandler handler)
        {
            if (!_globalHandlers.Contains(handler))
            {
                _globalHandlers.Add(handler);
                _globalHandlers.Sort((a, b) => a.ExecutionOrder.CompareTo(b.ExecutionOrder));
                _logger.LogDebug("Registered global handler: {HandlerName}", handler.HandlerName);
            }
        }

        /// <summary>
        /// تسجيل معالج ذكي
        /// </summary>
        private void RegisterAutoHandlerInternal(IAutoEventHandler handler)
        {
            if (!_autoHandlers.Contains(handler))
            {
                _autoHandlers.Add(handler);
                _autoHandlers.Sort((a, b) => a.ExecutionOrder.CompareTo(b.ExecutionOrder));

                // تسجيل أنواع الأحداث التي يعالجها
                var eventTypes = handler.GetHandledEventTypes();
                foreach (var eventType in eventTypes)
                {
                    RegisterHandlerForEventType(handler, eventType);
                }

                _logger.LogDebug("Registered auto handler: {HandlerName} for {EventCount} event types",
                    handler.HandlerName, eventTypes.Count());
            }
        }

        /// <summary>
        /// اكتشاف تلقائي لأنواع الأحداث من المعالج
        /// </summary>
        private void DiscoverAndRegisterEventTypes(object handlerInstance)
        {
            var handlerType = handlerInstance.GetType();
            var discoveredEventTypes = DiscoverHandledEventTypes(handlerType);

            if (!discoveredEventTypes.Any())
            {
                _logger.LogWarning("No event types discovered for handler: {HandlerType}", handlerType.Name);
                return;
            }

            foreach (var eventType in discoveredEventTypes)
            {
                RegisterHandlerForEventType(handlerInstance, eventType);
            }

            _logger.LogDebug("Discovered and registered {EventCount} event types for handler: {HandlerType}",
                discoveredEventTypes.Count(), handlerType.Name);
        }

        /// <summary>
        /// اكتشاف أنواع الأحداث التي يعالجها نوع المعالج
        /// </summary>
        private IEnumerable<Type> DiscoverHandledEventTypes(Type handlerType)
        {
            var eventTypes = new HashSet<Type>();

            // 1. البحث في الواجهات المباشرة
            foreach (var interfaceType in handlerType.GetInterfaces())
            {
                if (interfaceType.IsGenericType &&
                    interfaceType.GetGenericTypeDefinition() == typeof(IEventHandler<>))
                {
                    eventTypes.Add(interfaceType.GetGenericArguments()[0]);
                }
            }

            // 2. البحث في الفئات الأساسية
            var baseType = handlerType.BaseType;
            while (baseType != null && baseType != typeof(object))
            {
                if (baseType.IsGenericType)
                {
                    var genericTypeDef = baseType.GetGenericTypeDefinition();

                    // البحث عن SmartEventHandler<T> أو FlexibleEventHandler
                    if (genericTypeDef == typeof(SmartEventHandler<>) ||
                        genericTypeDef == typeof(FlexibleEventHandler) ||
                        genericTypeDef.IsSubclassOf(typeof(SmartEventHandler<>)))
                    {
                        if (baseType.GetGenericArguments().Length > 0)
                        {
                            eventTypes.Add(baseType.GetGenericArguments()[0]);
                        }
                    }
                }

                // البحث في واجهات الفئة الأساسية
                foreach (var interfaceType in baseType.GetInterfaces())
                {
                    if (interfaceType.IsGenericType &&
                        interfaceType.GetGenericTypeDefinition() == typeof(IEventHandler<>))
                    {
                        eventTypes.Add(interfaceType.GetGenericArguments()[0]);
                    }
                }

                baseType = baseType.BaseType;
            }

            return eventTypes;
        }

        /// <summary>
        /// تسجيل معالج لنوع حدث محدد
        /// </summary>
        private void RegisterHandlerForEventType(object handler, Type eventType)
        {
            if (!typeof(BaseEvent).IsAssignableFrom(eventType))
            {
                _logger.LogWarning("Invalid event type {EventType} for handler {HandlerType}",
                    eventType.Name, handler.GetType().Name);
                return;
            }

            if (!_typedHandlers.ContainsKey(eventType))
            {
                _typedHandlers[eventType] = new List<object>();
            }

            var handlersForEvent = _typedHandlers[eventType];
            if (!handlersForEvent.Contains(handler))
            {
                handlersForEvent.Add(handler);

                // ترتيب المعالجات حسب ExecutionOrder
                handlersForEvent.Sort((a, b) =>
                {
                    var orderA = GetExecutionOrder(a);
                    var orderB = GetExecutionOrder(b);
                    return orderA.CompareTo(orderB);
                });

                _logger.LogTrace("Registered handler {HandlerType} for event {EventType}",
                    handler.GetType().Name, eventType.Name);
            }
        }

        private int GetExecutionOrder(object handler)
        {
            try
            {
                var property = handler.GetType().GetProperty("ExecutionOrder",
                    BindingFlags.Public | BindingFlags.Instance);
                if (property != null && property.PropertyType == typeof(int))
                {
                    return (int)property.GetValue(handler);
                }
            }
            catch
            {
                // تجاهل الأخطاء
            }

            return 100; // القيمة الافتراضية
        }

        public void RegisterGlobalHandler(IGlobalEventHandler handler)
        {
            lock (_syncLock)
            {
                RegisterGlobalHandlerInternal(handler);
            }
        }

        public void UnregisterGlobalHandler(IGlobalEventHandler handler)
        {
            lock (_syncLock)
            {
                _globalHandlers.Remove(handler);
                _logger.LogDebug("Unregistered global handler: {HandlerName}", handler.HandlerName);
            }
        }

        public IReadOnlyList<IEventHandler<TEvent>> GetHandlers<TEvent>()
            where TEvent : BaseEvent
        {
            var eventType = typeof(TEvent);

            if (_typedHandlers.TryGetValue(eventType, out var handlers))
            {
                return handlers
                    .OfType<IEventHandler<TEvent>>()
                    .ToList()
                    .AsReadOnly();
            }

            // البحث في المعالجات الذكية التي تدعم هذا النوع
            var autoHandlers = _autoHandlers
                .Where(h => h.GetHandledEventTypes().Contains(eventType))
                .OfType<IEventHandler<TEvent>>()
                .ToList();

            return autoHandlers.AsReadOnly();
        }

        public IReadOnlyList<IGlobalEventHandler> GetGlobalHandlers()
        {
            lock (_syncLock)
            {
                return _globalHandlers.AsReadOnly();
            }
        }

        public IReadOnlyList<IAutoEventHandler> GetHandlersForEvent(BaseEvent @event)
        {
            var result = new List<IAutoEventHandler>();

            // إضافة المعالجات الذكية التي تدعم هذا الحدث
            var eventType = @event.GetType();
            foreach (var autoHandler in _autoHandlers)
            {
                if (autoHandler.GetHandledEventTypes().Contains(eventType) ||
                    autoHandler.CanHandle(@event.EventType))
                {
                    result.Add(autoHandler);
                }
            }

            // إضافة المعالجات العامة إذا كانت تدعم هذا الحدث
            foreach (var globalHandler in _globalHandlers)
            {
                if (globalHandler.CanHandle(@event.EventType))
                {
                    result.Add(globalHandler);
                }
            }

            // ترتيب حسب ExecutionOrder
            result.Sort((a, b) => a.ExecutionOrder.CompareTo(b.ExecutionOrder));

            return result.AsReadOnly();
        }

        public bool HasHandlers<TEvent>() where TEvent : BaseEvent
        {
            var eventType = typeof(TEvent);

            if (_typedHandlers.ContainsKey(eventType) && _typedHandlers[eventType].Count > 0)
                return true;

            return _autoHandlers.Any(h => h.GetHandledEventTypes().Contains(eventType));
        }

        public void Clear()
        {
            lock (_syncLock)
            {
                _typedHandlers.Clear();
                _globalHandlers.Clear();
                _autoHandlers.Clear();
                _logger.LogInformation("Cleared all registered handlers");
            }
        }

        public int GetTotalHandlerCount()
        {
            lock (_syncLock)
            {
                int count = _globalHandlers.Count + _autoHandlers.Count;
                foreach (var handlers in _typedHandlers.Values)
                {
                    count += handlers.Count;
                }
                return count;
            }
        }

        /// <summary>
        /// الحصول على إحصائيات مفصلة عن التسجيل
        /// </summary>
        public RegistryStatistics GetStatistics()
        {
            lock (_syncLock)
            {
                return new RegistryStatistics
                {
                    TotalGlobalHandlers = _globalHandlers.Count,
                    TotalAutoHandlers = _autoHandlers.Count,
                    TotalTypedHandlers = _typedHandlers.Values.Sum(h => h.Count),
                    RegisteredEventTypes = _typedHandlers.Keys.Select(k => k.Name).ToList(),
                    TotalHandlers = GetTotalHandlerCount()
                };
            }
        }

        /// <summary>
        /// البحث عن جميع المعالجات التي تعالج نوع حدث معين
        /// </summary>
        public IEnumerable<object> FindHandlersByEventType(Type eventType)
        {
            var handlers = new List<object>();

            // المعالجات النوعية
            if (_typedHandlers.TryGetValue(eventType, out var typedHandlers))
            {
                handlers.AddRange(typedHandlers);
            }

            // المعالجات الذكية
            handlers.AddRange(_autoHandlers
                .Where(h => h.GetHandledEventTypes().Contains(eventType)));

            return handlers;
        }
    }
}
