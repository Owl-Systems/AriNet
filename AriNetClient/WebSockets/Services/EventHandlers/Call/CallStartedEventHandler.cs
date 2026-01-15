using AriNetClient.WebSockets.Events.Call;
using Microsoft.Extensions.Logging;

namespace AriNetClient.WebSockets.Services.EventHandlers.Call
{
    /// <summary>
    /// معالج حدث بدء المكالمة
    /// </summary>
    public class CallStartedEventHandler : BaseEventHandler<CallStartedEvent>
    {
        private readonly ILogger<CallStartedEventHandler> _logger;

        public CallStartedEventHandler(ILogger<CallStartedEventHandler> logger)
            : base(logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// ترتيب تنفيذ أقل ليتم تنفيذه أولاً
        /// </summary>
        public override int ExecutionOrder => 10;

        protected override async Task ProcessEventAsync(CallStartedEvent @event, CancellationToken cancellationToken)
        {
            // تسجيل الحدث
            _logger.LogInformation("Call started - ID: {CallId}, From: {Caller}, To: {Callee}",
                @event.CallId, @event.CallerNumber, @event.CalleeNumber);

            // هنا يمكن إضافة منطق الأعمال مثل:
            // - تحديث حالة العميل في CRM
            // - إشعار المشغلين
            // - تسجيل بداية المكالمة في قاعدة البيانات

            // مثال: تحديث لوحة التحكم
            await UpdateCallDashboardAsync(@event, cancellationToken);

            // مثال: إرسال إشعار
            await SendNotificationAsync(@event, cancellationToken);

            // تنفيذ أي منطق إضافي
            await OnCallStartedAsync(@event, cancellationToken);
        }

        private async Task UpdateCallDashboardAsync(CallStartedEvent @event, CancellationToken cancellationToken)
        {
            // منطق تحديث لوحة التحكم
            _logger.LogDebug("Updating dashboard for call {CallId}", @event.CallId);
            await Task.Delay(100, cancellationToken); // محاكاة عملية غير متزامنة
        }

        private async Task SendNotificationAsync(
            CallStartedEvent @event,
            CancellationToken cancellationToken)
        {
            // منطق إرسال الإشعارات
            _logger.LogDebug("Sending notification for call {CallId}", @event.CallId);
            await Task.Delay(100, cancellationToken); // محاكاة عملية غير متزامنة
        }

        /// <summary>
        /// طريقة افتراضية يمكن للفئات المشتقة تخطيها
        /// </summary>
        protected virtual Task OnCallStartedAsync(CallStartedEvent @event, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

}
