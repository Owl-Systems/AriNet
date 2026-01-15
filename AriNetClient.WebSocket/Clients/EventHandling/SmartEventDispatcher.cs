using AriNetClient.WebSocket;
using AriNetClient.WebSocket.Statistics;
using AriNetClient.WebSockets.Events;
using AriNetClient.WebSockets.Events.Call;
using Microsoft.Extensions.Logging;

namespace AriNetClient.WebSockets.Clients.EventHandling
{
    /// <summary>
    /// موزع الأحداث الذكي - النسخة المحسنة
    /// </summary>
    public class SmartEventDispatcher : IEventDispatcher
    {
        private readonly IEventHandlerRegistry _registry;
        private readonly ILogger<SmartEventDispatcher> _logger;
        private readonly IServiceProvider _serviceProvider;

        public SmartEventDispatcher(
            IEventHandlerRegistry registry,
            ILogger<SmartEventDispatcher> logger,
            IServiceProvider serviceProvider)
        {
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

            _logger.LogDebug("SmartEventDispatcher initialized");
        }

        /// <summary>
        /// تسجيل معالج تلقائياً
        /// </summary>
        public void RegisterHandler<THandler>() where THandler : class
        {
            try
            {
                _registry.RegisterHandler<THandler>();
                _logger.LogDebug("Auto-registered handler: {HandlerType}", typeof(THandler).Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to auto-register handler: {HandlerType}", typeof(THandler).Name);
                throw;
            }
        }

        /// <summary>
        /// تسجيل معالج عام
        /// </summary>
        public void RegisterGlobalHandler(IGlobalEventHandler handler)
        {
            try
            {
                _registry.RegisterGlobalHandler(handler);
                _logger.LogDebug("Registered global handler: {HandlerName}", handler.HandlerName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to register global handler: {HandlerName}", handler.HandlerName);
                throw;
            }
        }

        /// <summary>
        /// إلغاء تسجيل معالج عام
        /// </summary>
        public void UnregisterGlobalHandler(IGlobalEventHandler handler)
        {
            try
            {
                _registry.UnregisterGlobalHandler(handler);
                _logger.LogDebug("Unregistered global handler: {HandlerName}", handler.HandlerName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to unregister global handler: {HandlerName}", handler.HandlerName);
                throw;
            }
        }

        /// <summary>
        /// توزيع حدث إلى جميع المعالجات المناسبة
        /// </summary>
        public async Task DispatchAsync(
            BaseEvent @event,
            CancellationToken cancellationToken = default)
        {
            if (@event == null)
                throw new ArgumentNullException(nameof(@event));

            var eventTypeName = @event.EventType ?? "unknown";
            var eventId = GetEventIdentifier(@event);

            _logger.LogDebug("Dispatching event: {EventType} ({EventId})", eventTypeName, eventId);

            try
            {
                // 1. إحصائيات الأداء
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();

                // 2. توزيع إلى المعالجات العامة أولاً
                await DispatchToGlobalHandlersAsync(@event, cancellationToken);

                // 3. توزيع إلى المعالجات النوعية
                await DispatchToTypedHandlersAsync(@event, cancellationToken);

                // 4. توزيع إلى المعالجات الذكية
                await DispatchToAutoHandlersAsync(@event, cancellationToken);

                stopwatch.Stop();

                _logger.LogDebug("Event {EventType} dispatched in {ElapsedMs}ms",
                    eventTypeName, stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to dispatch event: {EventType}", eventTypeName);
                throw;
            }
        }

        /// <summary>
        /// توزيع الحدث إلى المعالجات العامة
        /// </summary>
        private async Task DispatchToGlobalHandlersAsync(
            BaseEvent @event,
            CancellationToken cancellationToken)
        {
            var globalHandlers = _registry.GetGlobalHandlers();

            if (!globalHandlers.Any())
            {
                _logger.LogTrace("No global handlers registered for event: {EventType}", @event.EventType);
                return;
            }

            _logger.LogDebug("Dispatching to {Count} global handlers for event: {EventType}",
                globalHandlers.Count, @event.EventType);

            foreach (var handler in globalHandlers)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                if (!handler.CanHandle(@event.EventType))
                {
                    _logger.LogTrace("Global handler {HandlerName} cannot handle event: {EventType}",
                        handler.HandlerName, @event.EventType);
                    continue;
                }

                await ExecuteHandlerSafelyAsync(handler, @event, cancellationToken, "global");
            }
        }

        /// <summary>
        /// توزيع الحدث إلى المعالجات النوعية
        /// </summary>
        private async Task DispatchToTypedHandlersAsync(
            BaseEvent @event,
            CancellationToken cancellationToken)
        {
            // الحصول على المعالجات المناسبة لنوع الحدث
            var handlers = GetTypedHandlersForEvent(@event);

            if (!handlers.Any())
            {
                _logger.LogTrace("No typed handlers found for event: {EventType}", @event.EventType);
                return;
            }

            _logger.LogDebug("Dispatching to {Count} typed handlers for event: {EventType}",
                handlers.Count, @event.EventType);

            foreach (var handler in handlers)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                await ExecuteHandlerSafelyAsync(handler, @event, cancellationToken, "typed");
            }
        }

        /// <summary>
        /// الحصول على المعالجات النوعية للحدث
        /// </summary>
        private List<IAutoEventHandler> GetTypedHandlersForEvent(BaseEvent @event)
        {
            var handlers = _registry.GetHandlersForEvent(@event);
            return handlers.ToList();
        }

        /// <summary>
        /// توزيع الحدث إلى المعالجات الذكية
        /// </summary>
        private async Task DispatchToAutoHandlersAsync(
            BaseEvent @event,
            CancellationToken cancellationToken)
        {
            // يتم التعامل مع المعالجات الذكية في GetHandlersForEvent
            // لذا هذه الدالة احتياطية لأي معالجات إضافية
            await Task.CompletedTask;
        }

        /// <summary>
        /// تنفيذ معالج بأمان مع التعامل مع الأخطاء
        /// </summary>
        private async Task ExecuteHandlerSafelyAsync(
            IAutoEventHandler handler,
            BaseEvent @event,
            CancellationToken cancellationToken,
            string handlerType)
        {
            try
            {
                _logger.LogTrace("Executing {HandlerType} handler: {HandlerName} for event: {EventType}",
                    handlerType, handler.HandlerName, @event.EventType);

                await handler.HandleAsync(@event, cancellationToken);

                _logger.LogTrace("Completed {HandlerType} handler: {HandlerName} for event: {EventType}",
                    handlerType, handler.HandlerName, @event.EventType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "{HandlerType} handler {HandlerName} failed to handle event {EventType}",
                    handlerType, handler.HandlerName, @event.EventType);

                // يمكن إضافة منطق لإعادة المحاولة أو التعافي هنا
                if (IsCriticalException(ex))
                {
                    throw;
                }
            }
        }

        private bool IsCriticalException(Exception ex)
        {
            return ex is OutOfMemoryException
                || ex is StackOverflowException
                || ex is ThreadAbortException;
        }

        private string GetEventIdentifier(BaseEvent @event)
        {
            // محاولة الحصول على معرف فريد للحدث
            return @event switch
            {
                CallStartedEvent callStarted => callStarted.CallId,
                CallUpdatedEvent callUpdated => callUpdated.CallId,
                CallEndedEvent callEnded => callEnded.CallId,
                _ => @event.Data?.GetValueOrDefault("id")?.ToString()
                    ?? @event.Data?.GetValueOrDefault("call_id")?.ToString()
                    ?? "unknown"
            };
        }

        public int GetHandlerCount<TEvent>() where TEvent : BaseEvent
        {
            var handlers = _registry.GetHandlers<TEvent>();
            return handlers.Count;
        }

        public int GetGlobalHandlerCount()
        {
            var globalHandlers = _registry.GetGlobalHandlers();
            return globalHandlers.Count;
        }

        /// <summary>
        /// الحصول على إحصائيات التوزيع
        /// </summary>
        public DispatcherStatistics GetStatistics()
        {
            var typedCount = GetHandlerCount<BaseEvent>(); // تقديري
            var globalCount = GetGlobalHandlerCount();

            return new DispatcherStatistics
            {
                TotalTypedHandlers = typedCount,
                TotalGlobalHandlers = globalCount,
                TotalHandlers = typedCount + globalCount
            };
        }
    }


}
