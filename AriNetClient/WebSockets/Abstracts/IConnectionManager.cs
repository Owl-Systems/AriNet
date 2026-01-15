using System.Net.WebSockets;

namespace AriNetClient.WebSockets.Abstracts
{
    /// <summary>
    /// واجهة مدير الاتصال المسؤول عن إدارة الاتصال الفعلي بـ WebSocket
    /// </summary>
    public interface IConnectionManager : IDisposable
    {
        /// <summary>
        /// حدث الاتصال الناجح
        /// </summary>
        event EventHandler Connected;

        /// <summary>
        /// حدث فقدان الاتصال
        /// </summary>
        event EventHandler<string> Disconnected;

        /// <summary>
        /// حدث استقبال رسالة
        /// </summary>
        event EventHandler<string> MessageReceived;

        /// <summary>
        /// حدث خطأ
        /// </summary>
        event EventHandler<Exception> ErrorOccurred;

        /// <summary>
        /// حالة الاتصال الحالية
        /// </summary>
        WebSocketState State { get; }

        /// <summary>
        /// هل الاتصال نشط؟
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// الاتصال بخادم WebSocket
        /// </summary>
        Task ConnectAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// قطع الاتصال بخادم WebSocket
        /// </summary>
        Task DisconnectAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// إرسال رسالة إلى الخادم
        /// </summary>
        Task SendAsync(string message, CancellationToken cancellationToken = default);

        /// <summary>
        /// إعادة الاتصال إذا كان مفصولاً
        /// </summary>
        Task<bool> ReconnectAsync(CancellationToken cancellationToken = default);
    }
}
