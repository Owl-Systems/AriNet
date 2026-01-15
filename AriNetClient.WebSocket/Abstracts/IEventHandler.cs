using AriNetClient.WebSockets.Events;

namespace AriNetClient.WebSocket
{
    /// <summary>
    /// واجهة أساسية لمعالجات الأحداث النوعية
    /// </summary>
    public interface IEventHandler<TEvent> where TEvent : BaseEvent
    {
        /// <summary>
        /// اسم المعالج
        /// </summary>
        string HandlerName { get; }

        /// <summary>
        /// ترتيب التنفيذ
        /// </summary>
        int ExecutionOrder { get; }

        /// <summary>
        /// معالجة الحدث
        /// </summary>
        Task HandleAsync(TEvent @event, CancellationToken cancellationToken = default);
    }
}
