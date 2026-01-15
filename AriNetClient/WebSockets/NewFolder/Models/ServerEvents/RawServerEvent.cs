using System.Text.Json;

namespace AriNetClient.WebSockets.NewFolder.Models.ServerEvents
{
    /// <summary>
    /// حدث خادم أولي - يمثل البيانات الخام كما ترد من WebSocket
    /// هذا لتمثيل النقل (Transport) فقط
    /// </summary>
    public class RawServerEvent
    {
        /// <summary>
        /// تسمية الحدث كما أرسلها الخادم
        /// </summary>
        public string ServerEventType { get; }

        /// <summary>
        /// الطابع الزمني كما أرسله الخادم
        /// </summary>
        public DateTime ServerTimestamp { get; }

        /// <summary>
        /// اسم التطبيق في الخادم
        /// </summary>
        public string ServerApplication { get; }

        /// <summary>
        /// البيانات الأولية كما أرسلها الخادم
        /// </summary>
        public JsonDocument RawData { get; }

        /// <summary>
        /// معرف الخادم الذي أرسل الحدث (مفيد عند دعم مزودين متعددين)
        /// </summary>
        public string ServerIdentifier { get; }

        public RawServerEvent(
            string serverEventType,
            DateTime serverTimestamp,
            string serverApplication,
            JsonDocument rawData,
            string serverIdentifier)
        {
            ServerEventType = serverEventType ?? throw new ArgumentNullException(nameof(serverEventType));
            ServerTimestamp = serverTimestamp;
            ServerApplication = serverApplication;
            RawData = rawData ?? throw new ArgumentNullException(nameof(rawData));
            ServerIdentifier = serverIdentifier ?? throw new ArgumentNullException(nameof(serverIdentifier));
        }

        public void Dispose()
        {
            RawData?.Dispose();
        }
    }
}
