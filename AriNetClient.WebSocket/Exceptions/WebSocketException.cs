namespace AriNetClient.Exceptions
{
    /// <summary>
    /// استثناء أساسي لأخطاء WebSocket
    /// </summary>
    public class WebSocketException : Exception
    {
        /// <summary>
        /// حالة اتصال WebSocket عند حدوث الخطأ
        /// </summary>
        public System.Net.WebSockets.WebSocketState ConnectionState { get; }

        /// <summary>
        /// عنوان URL الذي حدث فيه الخطأ
        /// </summary>
        public string Url { get; }

        public WebSocketException(string message, System.Net.WebSockets.WebSocketState connectionState, string url)
            : base(message)
        {
            ConnectionState = connectionState;
            Url = url;
        }

        public WebSocketException(string message, System.Net.WebSockets.WebSocketState connectionState,
            string url, Exception innerException)
            : base(message, innerException)
        {
            ConnectionState = connectionState;
            Url = url;
        }
    }

    /// <summary>
    /// استثناء فشل الاتصال بـ WebSocket
    /// </summary>
    public class WebSocketConnectionException : WebSocketException
    {
        public int RetryCount { get; }
        public int MaxRetries { get; }

        public WebSocketConnectionException(string url, int retryCount, int maxRetries, Exception innerException = null)
            : base($"Failed to connect to WebSocket after {retryCount} attempts",
                  System.Net.WebSockets.WebSocketState.None, url, innerException)
        {
            RetryCount = retryCount;
            MaxRetries = maxRetries;
        }
    }

    /// <summary>
    /// استثناء فقدان الاتصال بـ WebSocket
    /// </summary>
    public class WebSocketDisconnectedException : WebSocketException
    {
        public WebSocketDisconnectedException(string url, System.Net.WebSockets.WebSocketState lastState)
            : base("WebSocket connection was disconnected", lastState, url)
        {
        }
    }

    /// <summary>
    /// استثناء لإعادة الاتصال الفاشلة
    /// </summary>
    public class WebSocketReconnectionFailedException : WebSocketException
    {
        public TimeSpan NextRetryDelay { get; }

        public WebSocketReconnectionFailedException(string url, TimeSpan nextRetryDelay, Exception innerException = null)
            : base($"WebSocket reconnection failed, next retry in {nextRetryDelay.TotalSeconds} seconds",
                  System.Net.WebSockets.WebSocketState.Closed, url, innerException)
        {
            NextRetryDelay = nextRetryDelay;
        }
    }

    /// <summary>
    /// استثناء حدث غير معروف
    /// </summary>
    public class UnknownEventException : Exception
    {
        public string EventType { get; }
        public string EventJson { get; }

        public UnknownEventException(string eventType, string eventJson)
            : base($"Unknown event type received: {eventType}")
        {
            EventType = eventType;
            EventJson = eventJson;
        }
    }

    /// <summary>
    /// استثناء معالج غير مسجل
    /// </summary>
    public class EventHandlerNotRegisteredException : Exception
    {
        public Type EventType { get; }

        public EventHandlerNotRegisteredException(Type eventType)
            : base($"No event handler registered for event type: {eventType.Name}")
        {
            EventType = eventType;
        }
    }
}
