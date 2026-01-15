using AriNetClient.WebSockets.Events.Call;
using Microsoft.Extensions.Logging;

namespace AriNetClient.WebSockets.Services.EventHandlers.Call
{
    /// <summary>
    /// معالج حدث انتهاء المكالمة
    /// </summary>
    public class CallEndedEventHandler : BaseEventHandler<CallEndedEvent>
    {
        private readonly ILogger<CallEndedEventHandler> _logger;

        public CallEndedEventHandler(ILogger<CallEndedEventHandler> logger) : base(logger)
        {
            _logger = logger;
        }

        public override int ExecutionOrder => 30;

        protected override async Task ProcessEventAsync(CallEndedEvent @event, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Call ended - ID: {CallId}, Duration: {TotalDuration}s, Reason: {HangupCause}",
                @event.CallId, @event.TotalDuration, @event.HangupCause);

            // تسجيل النتائج النهائية للمكالمة
            await RecordCallResultAsync(@event, cancellationToken);

            // إنشاء تقرير المكالمة
            await GenerateCallReportAsync(@event, cancellationToken);

            // تنظيف الموارد المرتبطة بالمكالمة
            await CleanupCallResourcesAsync(@event, cancellationToken);

            // تنبيه الأنظمة الأخرى بانتهاء المكالمة
            await NotifySystemsOfCallEndAsync(@event, cancellationToken);
        }

        private async Task RecordCallResultAsync(CallEndedEvent @event, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Recording result for call {CallId}", @event.CallId);

            // منطق تسجيل النتائج
            bool success = @event.HangupCause == "NORMAL_CLEARING";
            string result = success ? "Completed successfully" : $"Failed: {@event.HangupCause}";

            _logger.LogInformation("Call {CallId} result: {Result}", @event.CallId, result);

            await Task.Delay(100, cancellationToken);
        }

        private async Task GenerateCallReportAsync(CallEndedEvent @event, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Generating report for call {CallId}", @event.CallId);

            // منطق إنشاء التقارير
            var reportData = new
            {
                CallId = @event.CallId,
                StartTime = @event.StartTime,
                EndTime = @event.EndTime,
                TotalDuration = @event.TotalDuration,
                TalkDuration = @event.TalkDuration,
                HangupCause = @event.HangupCause,
                IsRecorded = @event.IsRecorded,
                RecordingPath = @event.RecordingPath
            };

            // هنا يمكن حفظ التقرير في قاعدة البيانات أو إرساله إلى نظام التقارير
            await Task.Delay(150, cancellationToken);
        }

        private async Task CleanupCallResourcesAsync(CallEndedEvent @event, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Cleaning up resources for call {CallId}", @event.CallId);

            // منطق تنظيف الموارد (مثل إغلاق الملفات المؤقتة، تحرير الذاكرة، إلخ)
            await Task.Delay(50, cancellationToken);
        }

        private async Task NotifySystemsOfCallEndAsync(CallEndedEvent @event, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Notifying systems of call end for {CallId}", @event.CallId);

            // منطق إشعار الأنظمة الأخرى (CRM، أنظمة التسجيل، إلخ)
            await Task.Delay(100, cancellationToken);
        }
    }

}
