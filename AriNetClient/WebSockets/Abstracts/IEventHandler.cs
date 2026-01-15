using AriNetClient.WebSockets.Events;

namespace AriNetClient.WebSockets.Abstracts
{
    /// <summary>
    /// واجهة أساسية لمعالجات الأحداث
    /// </summary>
    public interface IEventHandler<TEvent> where TEvent : BaseEvent
    {
        /// <summary>
        /// اسم المعالج (لأغراض التسجيل والتتبع)
        /// </summary>
        string HandlerName { get; }

        /// <summary>
        /// ترتيب التنفيذ (للتحكم في تسلسل المعالجات)
        /// </summary>
        int ExecutionOrder { get; }

        /// <summary>
        /// معالجة الحدث
        /// </summary>
        Task HandleAsync(TEvent @event, CancellationToken cancellationToken = default);
    }
}
