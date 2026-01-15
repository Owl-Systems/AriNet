using AriNetClient.WebSockets.NewFolder.Abstracts;
using AriNetClient.WebSockets.NewFolder.Models.DomainEvents;
using AriNetClient.WebSockets.NewFolder.Models.ServerEvents;
using Microsoft.Extensions.Logging;

namespace AriNetClient.WebSockets.NewFolder.Adapters
{
    /// <summary>
    /// محول أحداث Wazo
    /// يعرف تنسيق Wazo المحدد ويحوله إلى أحداث نطاق موحدة
    /// </summary>
    public class WazoEventAdapter : IServerEventAdapter
    {
        public string SupportedProvider => "Wazo";

        private readonly ILogger<WazoEventAdapter> _logger;

        public WazoEventAdapter(ILogger<WazoEventAdapter> logger)
        {
            _logger = logger;
        }

        public bool CanHandle(RawServerEvent rawEvent)
        {
            // يتحقق مما إذا كان الحدث من Wazo
            return rawEvent.ServerIdentifier == "Wazo" ||
                   rawEvent.ServerEventType?.StartsWith("call.") == true;
        }

        public async Task<IReadOnlyList<DomainEvent>> AdaptAsync(
            RawServerEvent rawEvent,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var domainEvents = new List<DomainEvent>();

                switch (rawEvent.ServerEventType)
                {
                    case "call.started":
                        var callStartedEvent = AdaptToCallStarted(rawEvent);
                        if (callStartedEvent != null)
                            domainEvents.Add(callStartedEvent);
                        break;

                    case "call.updated":
                        var callUpdatedEvent = AdaptToCallUpdated(rawEvent);
                        if (callUpdatedEvent != null)
                            domainEvents.Add(callUpdatedEvent);
                        break;

                    case "call.ended":
                        var callEndedEvent = AdaptToCallEnded(rawEvent);
                        if (callEndedEvent != null)
                            domainEvents.Add(callEndedEvent);
                        break;

                    default:
                        _logger.LogDebug("Ignoring unknown Wazo event: {EventType}",
                            rawEvent.ServerEventType);
                        break;
                }

                return domainEvents;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to adapt Wazo event: {EventType}",
                    rawEvent.ServerEventType);
                return Array.Empty<DomainEvent>();
            }
        }

        private CallStartedDomainEvent AdaptToCallStarted(RawServerEvent rawEvent)
        {
            // ✅ كل معرفة بتنسيق Wazo موجودة هنا فقط
            var root = rawEvent.RawData.RootElement;

            // استخراج البيانات بتسميات Wazo المحددة
            var callId = root.GetProperty("call_id").GetString();
            var callerNumber = root.GetProperty("caller_number").GetString();
            var calleeNumber = root.GetProperty("callee_number").GetString();
            var timestampStr = root.GetProperty("timestamp").GetString();

            // التحقق من صحة البيانات
            if (string.IsNullOrEmpty(callId) ||
                string.IsNullOrEmpty(callerNumber) ||
                string.IsNullOrEmpty(calleeNumber))
            {
                _logger.LogWarning("Invalid call.started event from Wazo");
                return null;
            }

            // تحويل إلى أنواع النطاق
            DateTime timestamp;
            if (!DateTime.TryParse(timestampStr, out timestamp))
            {
                timestamp = rawEvent.ServerTimestamp;
            }

            // إنشاء حدث النطاق النظيف
            return new CallStartedDomainEvent(
                callId: callId,
                caller: new PhoneNumber(callerNumber),
                callee: new PhoneNumber(calleeNumber),
                callStartTime: timestamp,
                context: root.GetProperty("context").GetString() ?? "default"
            );
        }

        private CallUpdatedDomainEvent AdaptToCallUpdated(RawServerEvent rawEvent)
        {
            // منطق تحويل مشابه لـ call.updated
            var root = rawEvent.RawData.RootElement;

            var callId = root.GetProperty("call_id").GetString();
            var newStateStr = root.GetProperty("new_state").GetString();
            var previousStateStr = root.GetProperty("previous_state").GetString();
            var duration = root.GetProperty("duration").GetInt32();
            var answered = root.GetProperty("answered").GetBoolean();

            // تحويل تسميات Wazo إلى أنواع نطاقنا
            CallState newState = MapWazoStateToDomainState(newStateStr);
            CallState previousState = MapWazoStateToDomainState(previousStateStr);

            return new CallUpdatedDomainEvent(
                callId: callId,
                previousState: previousState,
                newState: newState,
                duration: TimeSpan.FromSeconds(duration),
                isAnswered: answered
            );
        }

        private CallState MapWazoStateToDomainState(string wazoState)
        {
            return wazoState?.ToLower() switch
            {
                "ringing" => CallState.Ringing,
                "answered" => CallState.Answered,
                "bridged" => CallState.Bridged,
                "held" => CallState.Held,
                "hungup" => CallState.HungUp,
                _ => CallState.Ringing
            };
        }

        private CallEndedDomainEvent AdaptToCallEnded(RawServerEvent rawEvent)
        {
            // منطق تحويل لـ call.ended
            var root = rawEvent.RawData.RootElement;

            return new CallEndedDomainEvent(
                callId: root.GetProperty("call_id").GetString(),
                startTime: DateTime.Parse(root.GetProperty("start_time").GetString()),
                endTime: DateTime.Parse(root.GetProperty("end_time").GetString()),
                totalDuration: TimeSpan.FromSeconds(root.GetProperty("total_duration").GetInt32()),
                talkDuration: TimeSpan.FromSeconds(root.GetProperty("talk_duration").GetInt32()),
                hangupCause: MapWazoHangupCause(root.GetProperty("hangup_cause").GetString()),
                wasRecorded: root.GetProperty("recorded").GetBoolean()
            );
        }

        private HangupCause MapWazoHangupCause(string wazoCause)
        {
            return wazoCause switch
            {
                "NORMAL_CLEARING" => HangupCause.NormalClearing,
                "NO_ANSWER" => HangupCause.NoAnswer,
                "USER_BUSY" => HangupCause.Busy,
                _ => HangupCause.NormalClearing
            };
        }
    }
}
