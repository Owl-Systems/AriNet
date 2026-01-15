using AriNetClient.WebSockets.Abstracts;
using AriNetClient.WebSockets.Events;
using AriNetClient.WebSockets.Events.Call;
using Microsoft.Extensions.Logging;

namespace AriNetClient.WebSockets.Clients.EventHandling
{
    /// <summary>
    /// موزع الأحداث (تنفيذ IEventDispatcher)
    /// </summary>
    public class EventDispatcher : IEventDispatcher
    {
        private readonly IEventHandlerRegistry _registry;
        private readonly ILogger<EventDispatcher> _logger;
        private readonly IServiceProvider _serviceProvider;

        public EventDispatcher(IEventHandlerRegistry registry, ILogger<EventDispatcher> logger, IServiceProvider serviceProvider)
        {
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public void RegisterHandler<TEvent, THandler>() where TEvent : BaseEvent where THandler : IEventHandler<TEvent>
        {
            try
            {
                _registry.RegisterHandler<TEvent, THandler>();
                _logger.LogDebug("Registered handler {Handler} for event {Event}",
                    typeof(THandler).Name, typeof(TEvent).Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to register handler {Handler} for event {Event}",
                    typeof(THandler).Name, typeof(TEvent).Name);
                throw;
            }
        }

        public void UnregisterHandler<TEvent, THandler>() where TEvent : BaseEvent where THandler : Abstracts.IEventHandler<TEvent>
        {
            try
            {
                _registry.UnregisterHandler<TEvent, THandler>();
                _logger.LogDebug("Unregistered handler {Handler} for event {Event}",
                    typeof(THandler).Name, typeof(TEvent).Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to unregister handler {Handler} for event {Event}",
                    typeof(THandler).Name, typeof(TEvent).Name);
                throw;
            }
        }

        public void RegisterGlobalHandler(IGlobalEventHandler handler)
        {
            try
            {
                _registry.RegisterGlobalHandler(handler);
                _logger.LogDebug("Registered global handler: {Handler}", handler.HandlerName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to register global handler: {Handler}", handler.HandlerName);
                throw;
            }
        }

        public void UnregisterGlobalHandler(IGlobalEventHandler handler)
        {
            try
            {
                _registry.UnregisterGlobalHandler(handler);
                _logger.LogDebug("Unregistered global handler: {Handler}", handler.HandlerName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to unregister global handler: {Handler}", handler.HandlerName);
                throw;
            }
        }

        public async Task DispatchAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : BaseEvent
        {
            ArgumentNullException.ThrowIfNull(@event);

            _logger.LogDebug("Dispatching event: {EventType} ({EventId})", @event.EventType, GetEventId(@event));

            try
            {
                // معالجة المعالجات العامة أولاً
                await DispatchToGlobalHandlersAsync(@event, cancellationToken);

                // معالجة المعالجات الخاصة بالحدث
                await DispatchToSpecificHandlersAsync(@event, cancellationToken);

                _logger.LogDebug("Successfully dispatched event: {EventType}", @event.EventType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to dispatch event: {EventType}", @event.EventType);
                throw;
            }
        }

        private async Task DispatchToGlobalHandlersAsync<TEvent>(TEvent @event, CancellationToken cancellationToken) where TEvent : BaseEvent
        {
            var globalHandlers = _registry.GetGlobalHandlers();

            foreach (var handler in globalHandlers)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                try
                {
                    if (handler.CanHandle(@event.EventType))
                    {
                        _logger.LogTrace("Dispatching to global handler: {Handler}", handler.HandlerName);
                        await handler.HandleAsync(@event, cancellationToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Global handler {Handler} failed to handle event {EventType}",
                        handler.HandlerName, @event.EventType);

                    // يمكن إما إعادة رفع الاستثناء أو الاستمرار حسب الحاجة
                    if (IsCriticalException(ex))
                        throw;
                }
            }
        }

        private async Task DispatchToSpecificHandlersAsync<TEvent>(TEvent @event, CancellationToken cancellationToken) where TEvent : BaseEvent
        {
            var handlers = _registry.GetHandlers<TEvent>();

            if (handlers.Count == 0)
            {
                _logger.LogWarning("No specific handlers registered for event: {EventType}", @event.EventType);
                return;
            }

            _logger.LogDebug("Dispatching to {Count} specific handlers for event: {EventType}",
                handlers.Count, @event.EventType);

            foreach (var handler in handlers)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                try
                {
                    _logger.LogTrace("Dispatching to handler: {Handler}", handler.HandlerName);
                    await handler.HandleAsync(@event, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Handler {Handler} failed to handle event {EventType}",
                        handler.HandlerName, @event.EventType);

                    if (IsCriticalException(ex))
                        throw;
                }
            }
        }

        private string GetEventId<TEvent>(TEvent @event) where TEvent : BaseEvent
        {
            // محاولة الحصول على معرف فريد للحدث
            return @event switch
            {
                CallStartedEvent callStarted => callStarted.CallId,
                CallUpdatedEvent callUpdated => callUpdated.CallId,
                CallEndedEvent callEnded => callEnded.CallId,
                _ => @event.Data.GetValueOrDefault("id")?.ToString() ?? "unknown"
            };
        }

        private bool IsCriticalException(Exception ex)
        {
            // تحديد ما إذا كان الاستثناء حرجاً ويجب إيقاف المعالجة
            return ex is OutOfMemoryException
                || ex is StackOverflowException
                || ex is ThreadAbortException;
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
    }
}
