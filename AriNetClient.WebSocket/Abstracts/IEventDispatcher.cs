using AriNetClient.WebSockets.Events;

namespace AriNetClient.WebSocket
{
    /// <summary>
    /// واجهة موزع الأحداث الذكي
    /// </summary>
    public interface IEventDispatcher
    {
        /// <summary>
        /// تسجيل معالج تلقائياً
        /// </summary>
        void RegisterHandler<THandler>() where THandler : class;

        /// <summary>
        /// تسجيل معالج عام
        /// </summary>
        void RegisterGlobalHandler(IGlobalEventHandler handler);

        /// <summary>
        /// إلغاء تسجيل معالج عام
        /// </summary>
        void UnregisterGlobalHandler(IGlobalEventHandler handler);

        /// <summary>
        /// توزيع حدث إلى جميع المعالجات المناسبة
        /// </summary>
        Task DispatchAsync(BaseEvent @event, CancellationToken cancellationToken = default);

        /// <summary>
        /// الحصول على عدد المعالجات لحدث معين
        /// </summary>
        int GetHandlerCount<TEvent>() where TEvent : BaseEvent;

        /// <summary>
        /// الحصول على عدد المعالجات العامة
        /// </summary>
        int GetGlobalHandlerCount();
    }
}
