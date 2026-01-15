namespace AriNetClient.WebSockets.Configuration
{
    /// <summary>
    /// خيارات تهيئة WebSocket
    /// </summary>
    public class WebSocketOptions
    {
        /// <summary>
        /// عنوان URL لخادم WebSocket
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// توكن المصادقة
        /// </summary>
        public string AuthToken { get; set; }

        /// <summary>
        /// اسم التطبيق للاشتراك في الأحداث
        /// </summary>
        public string ApplicationName { get; set; } = "ari-net-client";

        /// <summary>
        /// تمكين إعادة الاتصال التلقائي
        /// </summary>
        public bool AutoReconnect { get; set; } = true;

        /// <summary>
        /// أقصى عدد محاولات إعادة الاتصال
        /// </summary>
        public int MaxReconnectionAttempts { get; set; } = 10;

        /// <summary>
        /// زمن الانتظار الأولي قبل إعادة الاتصال (بالمللي ثانية)
        /// </summary>
        public int InitialReconnectDelayMs { get; set; } = 1000;

        /// <summary>
        /// الزيادة النسبية في زمن الانتظار بين المحاولات
        /// </summary>
        public double ReconnectDelayMultiplier { get; set; } = 1.5;

        /// <summary>
        /// زمن الانتظار الأقصى بين المحاولات (بالمللي ثانية)
        /// </summary>
        public int MaxReconnectDelayMs { get; set; } = 30000;

        /// <summary>
        /// تمكين تسجيل الأحداث الواردة
        /// </summary>
        public bool EnableEventLogging { get; set; } = true;

        /// <summary>
        /// مستوى تسجيل الأحداث (Trace, Debug, Information)
        /// </summary>
        public string EventLogLevel { get; set; } = "Information";

        /// <summary>
        /// تمكين التحقق من صحة الأحداث
        /// </summary>
        public bool ValidateEvents { get; set; } = true;

        /// <summary>
        /// أنواع الأحداث المراد الاشتراك فيها (إذا كان فارغاً، يتم الاشتراك في جميع الأحداث)
        /// </summary>
        public List<string> SubscribeToEvents { get; set; } = new();

        /// <summary>
        /// أنواع الأحداث المستثناة من الاشتراك
        /// </summary>
        public List<string> ExcludeEvents { get; set; } = new();

        /// <summary>
        /// حجم المخزن المؤقت لاستقبال الرسائل (بالبايت)
        /// </summary>
        public int ReceiveBufferSize { get; set; } = 4096;

        /// <summary>
        /// مهلة إبقاء الاتصال نشطاً (Keep-Alive) بالثواني
        /// </summary>
        public int KeepAliveIntervalSeconds { get; set; } = 30;

        /// <summary>
        /// التحقق من صحة الخيارات
        /// </summary>
        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(Url))
                throw new ArgumentException("WebSocket URL is required", nameof(Url));

            if (string.IsNullOrWhiteSpace(AuthToken))
                throw new ArgumentException("Authentication token is required", nameof(AuthToken));

            if (MaxReconnectionAttempts < 0)
                throw new ArgumentException("Max reconnection attempts cannot be negative", nameof(MaxReconnectionAttempts));

            if (InitialReconnectDelayMs <= 0)
                throw new ArgumentException("Initial reconnect delay must be positive", nameof(InitialReconnectDelayMs));

            if (ReconnectDelayMultiplier < 1.0)
                throw new ArgumentException("Reconnect delay multiplier must be >= 1.0", nameof(ReconnectDelayMultiplier));
        }
    }
}
