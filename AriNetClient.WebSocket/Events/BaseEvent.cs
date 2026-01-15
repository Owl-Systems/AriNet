using System.Text.Json;
using System.Text.Json.Serialization;

namespace AriNetClient.WebSockets.Events
{
    /// <summary>
    /// الحدث الأساسي الذي ترسله منصة Wazo عبر WebSocket
    /// كل الأحداث ترث من هذا النموذج
    /// </summary>
    public abstract class BaseEvent
    {
        /// <summary>
        /// نوع الحدث (مثال: "call.started", "call.updated", "call.ended")
        /// </summary>
        [JsonPropertyName("event_type")]
        public string EventType { get; set; }

        /// <summary>
        /// وقت حدوث الحدث في الخادم
        /// </summary>
        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// معرف التطبيق الذي أطلق الحدث
        /// </summary>
        [JsonPropertyName("application")]
        public string Application { get; set; }

        /// <summary>
        /// البيانات الإضافية الخاصة بالحدث
        /// </summary>
        [JsonPropertyName("data")]
        public Dictionary<string, object> Data { get; set; } = new();

        /// <summary>
        /// تحويل البيانات إلى نموذج قوي النوع
        /// </summary>
        public virtual T GetDataAs<T>() where T : class, new()
        {
            try
            {
                var json = JsonSerializer.Serialize(Data);
                return JsonSerializer.Deserialize<T>(json);
            }
            catch
            {
                return new T();
            }
        }
    }
}
