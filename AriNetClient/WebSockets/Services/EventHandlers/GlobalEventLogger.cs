using AriNetClient.WebSockets.Abstracts;
using AriNetClient.WebSockets.Configuration;
using AriNetClient.WebSockets.Events;
using Microsoft.Extensions.Logging;

namespace AriNetClient.WebSockets.Services.EventHandlers
{
    /// <summary>
    /// معالج عام لتسجيل جميع الأحداث (لأغراض التصحيح والمراقبة)
    /// </summary>
    public class GlobalEventLogger : IGlobalEventHandler
    {
        private readonly ILogger<GlobalEventLogger> _logger;
        private readonly WebSocketOptions _options;

        public string HandlerName => nameof(GlobalEventLogger);
        public int ExecutionOrder => 1; // تنفيذه أولاً لتسجيل الحدث

        public GlobalEventLogger(ILogger<GlobalEventLogger> logger, WebSocketOptions options)
        {
            _logger = logger;
            _options = options;
        }

        public bool CanHandle(string eventType)
        {
            // يدعم جميع أنواع الأحداث
            return true;
        }

        public async Task HandleAsync(BaseEvent @event, CancellationToken cancellationToken)
        {
            // التحقق مما إذا كان التسجيل مفعلاً لهذا الحدث
            if (!_options.EnableEventLogging)
                return;

            // تسجيل الحدث حسب مستوى السجل المحدد
            switch (_options.EventLogLevel?.ToLower())
            {
                case "trace":
                    _logger.LogTrace("Event received - Type: {EventType}, Time: {Timestamp}, App: {Application}",
                        @event.EventType, @event.Timestamp, @event.Application);
                    break;

                case "debug":
                    _logger.LogDebug("Event received - Type: {EventType}, Time: {Timestamp}",
                        @event.EventType, @event.Timestamp);
                    break;

                case "information":
                default:
                    _logger.LogInformation("Event received - Type: {EventType}", @event.EventType);
                    break;
            }

            await Task.CompletedTask;
        }
    }
}
