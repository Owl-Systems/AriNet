using AriNetClient.WebSocket.Mappers;
using AriNetClient.WebSockets.Events;
using Microsoft.Extensions.Logging;

namespace AriNetClient.WebSocket.Services.EventHandlers
{
    /// <summary>
    /// معالج ذكي يدعم الاكتشاف التلقائي والمعالجة المرنة
    /// </summary>
    public abstract class SmartEventHandler<TEvent> : IAutoEventHandler where TEvent : BaseEvent
    {
        private readonly ILogger _logger;

        /// <summary>
        /// اسم المعالج (يتم تعيينه افتراضياً باسم الفئة)
        /// </summary>
        public virtual string HandlerName => GetType().Name;

        /// <summary>
        /// ترتيب التنفيذ (القيمة الافتراضية 100)
        /// </summary>
        public virtual int ExecutionOrder => 100;

        /// <summary>
        /// أنواع الأحداث التي يعالجها هذا المعالج (افتراضياً نوع الحدث المحدد)
        /// </summary>
        public virtual IEnumerable<Type> GetHandledEventTypes()
        {
            yield return typeof(TEvent);
        }

        /// <summary>
        /// التحقق مما إذا كان المعالج يدعم نوع حدث معين
        /// </summary>
        public virtual bool CanHandle(string eventType)
        {
            // التحقق مما إذا كان نوع الحدث يتوافق مع نوع TEvent
            var expectedEventType = EventTypeMapper.MapToEventType<TEvent>();
            return string.Equals(eventType, expectedEventType, StringComparison.OrdinalIgnoreCase);
        }

        protected SmartEventHandler(ILogger logger = null)
        {
            _logger = logger;
        }

        /// <summary>
        /// معالجة الحدث النوعي
        /// </summary>
        public async Task HandleAsync(TEvent @event, CancellationToken cancellationToken = default)
        {
            await ProcessEventAsync(@event, cancellationToken);
        }

        /// <summary>
        /// معالجة الحدث العام (من IAutoEventHandler)
        /// </summary>
        async Task IEventHandler<BaseEvent>.HandleAsync(
            BaseEvent @event,
            CancellationToken cancellationToken)
        {
            // تحويل إلى TEvent إذا كان متوافقاً
            if (@event is TEvent typedEvent)
            {
                await HandleAsync(typedEvent, cancellationToken);
            }
            else
            {
                _logger?.LogWarning(
                    "Handler {HandlerName} received incompatible event type: {EventType}, expected: {ExpectedType}",
                    HandlerName, @event.GetType().Name, typeof(TEvent).Name);
            }
        }

        /// <summary>
        /// عملية معالجة الحدث الفعلية
        /// </summary>
        protected abstract Task ProcessEventAsync(TEvent @event, CancellationToken cancellationToken);
    }
}
