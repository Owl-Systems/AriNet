using AriNetClient.WebSockets.NewFolder.Abstracts;
using AriNetClient.WebSockets.NewFolder.Models.DomainEvents;
using AriNetClient.WebSockets.NewFolder.Models.ServerEvents;
using AriNetClient.WebSockets.Services.EventHandlers.Call;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AriNetClient.WebSockets.NewFolder.EventDispatching
{
    /// <summary>
    /// موزع الأحداث الرئيسي
    /// مسئول عن:
    /// 1. استقبال الأحداث الخام من WebSocket
    /// 2. اختيار المحول المناسب
    /// 3. تحويلها إلى أحداث نطاق
    /// 4. توزيعها على المعالجات
    /// </summary>
    public class EventDispatcher : IEventDispatcher
    {
        private readonly IEnumerable<IServerEventAdapter> _adapters;
        private readonly ILogger<EventDispatcher> _logger;
        private readonly IServiceProvider _serviceProvider;

        // أحداث للنظام (للإشعارات)
        public event EventHandler<DomainEvent> DomainEventDispatched;
        public event EventHandler<RawServerEvent> RawEventReceived;

        public EventDispatcher(
            IEnumerable<IServerEventAdapter> adapters,
            ILogger<EventDispatcher> logger,
            IServiceProvider serviceProvider)
        {
            _adapters = adapters ?? throw new ArgumentNullException(nameof(adapters));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        /// <summary>
        /// العملية الرئيسية: إرسال حدث خام ومعالجته
        /// </summary>
        public async Task DispatchRawEventAsync(
            RawServerEvent rawEvent,
            CancellationToken cancellationToken = default)
        {
            if (rawEvent == null)
                throw new ArgumentNullException(nameof(rawEvent));

            _logger.LogDebug("Dispatching raw event: {EventType} from {Server}",
                rawEvent.ServerEventType, rawEvent.ServerIdentifier);

            // 1. إشعار النظام باستقبال حدث خام
            RawEventReceived?.Invoke(this, rawEvent);

            // 2. البحث عن محول مناسب
            var adapter = FindAdapterForEvent(rawEvent);
            if (adapter == null)
            {
                _logger.LogWarning("No adapter found for event: {EventType} from {Server}",
                    rawEvent.ServerEventType, rawEvent.ServerIdentifier);
                return;
            }

            _logger.LogDebug("Using adapter: {Adapter} for event", adapter.GetType().Name);

            try
            {
                // 3. التحويل إلى أحداث نطاق
                var domainEvents = await adapter.AdaptAsync(rawEvent, cancellationToken);

                // 4. توزيع كل حدث نطاق
                foreach (var domainEvent in domainEvents)
                {
                    await DispatchDomainEventAsync(domainEvent, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process event: {EventType}",
                    rawEvent.ServerEventType);
                throw;
            }
            finally
            {
                rawEvent.Dispose();
            }
        }

        /// <summary>
        /// توزيع حدث نطاق على المعالجات المسجلة
        /// </summary>
        private async Task DispatchDomainEventAsync(DomainEvent domainEvent, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Dispatching domain event: {EventType}", domainEvent.EventType);

            // إشعار النظام بحدث نطاق
            DomainEventDispatched?.Invoke(this, domainEvent);

            // هنا سيتم توزيع الحدث على معالجات النطاق
            // (سننفذ هذا في الخطوة التالية)

            // مثال: تنفيذ بسيط الآن
            switch (domainEvent)
            {
                case CallStartedDomainEvent callStarted:
                    await HandleCallStartedAsync(callStarted, cancellationToken);
                    break;

                case CallUpdatedDomainEvent callUpdated:
                    await HandleCallUpdatedAsync(callUpdated, cancellationToken);
                    break;

                case CallEndedDomainEvent callEnded:
                    await HandleCallEndedAsync(callEnded, cancellationToken);
                    break;
            }
        }

        /// <summary>
        /// البحث عن المحول المناسب للحدث
        /// </summary>
        private IServerEventAdapter FindAdapterForEvent(RawServerEvent rawEvent)
        {
            // ✅ لا يوجد if/switch على نوع الخادم
            // ✅ كل محول يقرر إذا كان يمكنه التعامل مع الحدث

            foreach (var adapter in _adapters)
            {
                if (adapter.CanHandle(rawEvent))
                {
                    return adapter;
                }
            }

            return null;
        }

        // طرق المعالجة المؤقتة (سيتم استبدالها بنظام معالجات)
        private async Task HandleCallStartedAsync(
            CallStartedDomainEvent @event,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Call started: {CallId}, From: {Caller}, To: {Callee}",
                @event.CallId, @event.Caller.Value, @event.Callee.Value);

            // الحصول على معالجات محددة من نظام DI
            var handlers = _serviceProvider.GetServices<ICallStartedHandler>();
            foreach (var handler in handlers)
            {
                await handler.HandleAsync(@event, cancellationToken);
            }
        }

        private async Task HandleCallUpdatedAsync(
            CallUpdatedDomainEvent @event,
            CancellationToken cancellationToken)
        {
            // معالجة مماثلة
            var handlers = _serviceProvider.GetServices<ICallUpdatedHandler>();
            foreach (var handler in handlers)
            {
                await handler.HandleAsync(@event, cancellationToken);
            }
        }

        private async Task HandleCallEndedAsync(CallEndedDomainEvent @event, CancellationToken cancellationToken)
        {
            // معالجة مماثلة
            var handlers = _serviceProvider.GetServices<CallEndedEventHandler>();
            foreach (var handler in handlers)
            {
                await handler.HandleAsync(@event, cancellationToken);
            }
        }
    }

    /// <summary>
    /// واجهة موزع الأحداث
    /// </summary>
    public interface IEventDispatcher
    {
        event EventHandler<DomainEvent> DomainEventDispatched;
        event EventHandler<RawServerEvent> RawEventReceived;

        Task DispatchRawEventAsync(
            RawServerEvent rawEvent,
            CancellationToken cancellationToken = default);
    }
}
