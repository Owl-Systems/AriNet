using AriNetClient.WebSockets.Events;

namespace AriNetClient.WebSockets.Abstracts
{
    /// <summary>
    /// واجهة موزع الأحداث المسؤول عن توجيه الأحداث إلى المعالجات المناسبة
    /// </summary>
    public interface IEventDispatcher
    {
        /// <summary>
        /// تسجيل معالج لحدث محدد
        /// </summary>
        void RegisterHandler<TEvent, THandler>() where TEvent : BaseEvent where THandler : IEventHandler<TEvent>;

        /// <summary>
        /// إلغاء تسجيل معالج لحدث محدد
        /// </summary>
        void UnregisterHandler<TEvent, THandler>() where TEvent : BaseEvent where THandler : IEventHandler<TEvent>;

        /// <summary>
        /// توزيع حدث إلى المعالجات المسجلة
        /// </summary>
        Task DispatchAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : BaseEvent;

        /// <summary>
        /// تسجيل معالج عام لجميع الأحداث
        /// </summary>
        void RegisterGlobalHandler(IGlobalEventHandler handler);

        /// <summary>
        /// إلغاء تسجيل معالج عام
        /// </summary>
        void UnregisterGlobalHandler(IGlobalEventHandler handler);

        /// <summary>
        /// الحصول على عدد المعالجات المسجلة لحدث معين
        /// </summary>
        int GetHandlerCount<TEvent>() where TEvent : BaseEvent;
    }
}
