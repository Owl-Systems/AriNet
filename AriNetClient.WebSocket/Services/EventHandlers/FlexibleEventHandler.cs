using AriNetClient.WebSocket.Mappers;
using AriNetClient.WebSockets.Events;
using Microsoft.Extensions.Logging;

namespace AriNetClient.WebSocket.Services.EventHandlers
{
    /// <summary>
    /// معالج أحداث مرن يمكنه معالجة أنواع متعددة
    /// </summary>
    public abstract class FlexibleEventHandler : IAutoEventHandler
    {
        private readonly ILogger _logger;

        public virtual string HandlerName => GetType().Name;
        public virtual int ExecutionOrder => 100;

        /// <summary>
        /// أنواع الأحداث التي يعالجها هذا المعالج (يجب تنفيذها في الفئات المشتقة)
        /// </summary>
        public abstract IEnumerable<Type> GetHandledEventTypes();

        /// <summary>
        /// التحقق مما إذا كان المعالج يدعم نوع حدث معين
        /// </summary>
        public virtual bool CanHandle(string eventType)
        {
            var mappedType = EventTypeMapper.MapToDotNetType(eventType);
            return GetHandledEventTypes().Any(t => t == mappedType);
        }

        protected FlexibleEventHandler(ILogger logger = null)
        {
            _logger = logger;
        }

        /// <summary>
        /// معالجة الحدث
        /// </summary>
        public async Task HandleAsync(BaseEvent @event, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger?.LogDebug(
                    "Flexible handler {HandlerName} processing event {EventType}",
                    HandlerName, @event.EventType);

                await ProcessEventAsync(@event, cancellationToken);

                _logger?.LogDebug(
                    "Flexible handler {HandlerName} completed processing event {EventType}",
                    HandlerName, @event.EventType);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex,
                    "Flexible handler {HandlerName} failed to process event {EventType}",
                    HandlerName, @event.EventType);
                throw;
            }
        }

        /// <summary>
        /// عملية معالجة الحدث الفعلية
        /// </summary>
        protected abstract Task ProcessEventAsync(BaseEvent @event, CancellationToken cancellationToken);
    }
}
