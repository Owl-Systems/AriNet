using AriNetClient.WebSockets.Abstracts;
using AriNetClient.WebSockets.Events;
using Microsoft.Extensions.Logging;

namespace AriNetClient.WebSockets.Services.EventHandlers
{
    /// <summary>
    /// معالج أساسي للأحداث يوفر وظائف مشتركة
    /// </summary>
    public abstract class BaseEventHandler<TEvent> : IEventHandler<TEvent> where TEvent : BaseEvent
    {
        protected readonly ILogger _logger;

        /// <summary>
        /// اسم المعالج (يتم تعيينه افتراضياً باسم الفئة)
        /// </summary>
        public virtual string HandlerName => GetType().Name;

        /// <summary>
        /// ترتيب التنفيذ (القيمة الافتراضية 100)
        /// </summary>
        public virtual int ExecutionOrder => 100;

        protected BaseEventHandler(ILogger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// تنفيذ معالجة الحدث مع التعامل مع الأخطاء
        /// </summary>
        public async Task HandleAsync(TEvent @event, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger?.LogDebug("Handler {HandlerName} started processing event {EventType}", HandlerName, @event.EventType);

                await ProcessEventAsync(@event, cancellationToken);

                _logger?.LogDebug("Handler {HandlerName} completed processing event {EventType}", HandlerName, @event.EventType);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Handler {HandlerName} failed to process event {EventType}", HandlerName, @event.EventType);

                // يمكن إعادة رفع الاستثناء أو معالجته حسب الحاجة
                throw;
            }
        }

        /// <summary>
        /// عملية معالجة الحدث الفعلية (يجب تنفيذها في الفئات المشتقة)
        /// </summary>
        protected abstract Task ProcessEventAsync(TEvent @event, CancellationToken cancellationToken);
    }
}
