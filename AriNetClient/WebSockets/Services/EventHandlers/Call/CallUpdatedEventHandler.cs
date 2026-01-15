using AriNetClient.WebSockets.Events.Call;
using Microsoft.Extensions.Logging;

namespace AriNetClient.WebSockets.Services.EventHandlers.Call
{
    /// <summary>
    /// معالج حدث تحديث المكالمة
    /// </summary>
    public class CallUpdatedEventHandler : BaseEventHandler<CallUpdatedEvent>
    {
        private readonly ILogger<CallUpdatedEventHandler> _logger;

        public CallUpdatedEventHandler(ILogger<CallUpdatedEventHandler> logger)
            : base(logger)
        {
            _logger = logger;
        }

        public override int ExecutionOrder => 20;

        protected override async Task ProcessEventAsync(CallUpdatedEvent @event, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Call updated - ID: {CallId}, State: {NewState} (Previous: {PreviousState})",
                @event.CallId, @event.NewState, @event.PreviousState);

            // معالجة حسب الحالة الجديدة
            switch (@event.NewState?.ToLower())
            {
                case "answered":
                    await OnCallAnsweredAsync(@event, cancellationToken);
                    break;

                case "bridged":
                    await OnCallBridgedAsync(@event, cancellationToken);
                    break;

                case "ringing":
                    await OnCallRingingAsync(@event, cancellationToken);
                    break;

                default:
                    _logger.LogDebug("Call {CallId} changed to unknown state: {NewState}",
                        @event.CallId, @event.NewState);
                    break;
            }

            // تحديث إحصائيات المكالمة
            await UpdateCallStatisticsAsync(@event, cancellationToken);
        }

        private async Task OnCallAnsweredAsync(CallUpdatedEvent @event, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Call {CallId} was answered after {Duration} seconds",
                @event.CallId, @event.Duration);

            // منطق الأعمال عند الرد على المكالمة
            await Task.Delay(100, cancellationToken);
        }

        private async Task OnCallBridgedAsync(CallUpdatedEvent @event, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Call {CallId} was bridged", @event.CallId);

            // منطق الأعمال عند ربط المكالمة
            await Task.Delay(100, cancellationToken);
        }

        private async Task OnCallRingingAsync(CallUpdatedEvent @event, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Call {CallId} is ringing", @event.CallId);

            // منطق الأعمال عند رنين المكالمة
            await Task.Delay(100, cancellationToken);
        }

        private async Task UpdateCallStatisticsAsync(CallUpdatedEvent @event, CancellationToken cancellationToken)
        {
            // منطق تحديث الإحصائيات
            _logger.LogDebug("Updating statistics for call {CallId}", @event.CallId);
            await Task.Delay(50, cancellationToken);
        }
    }


}
