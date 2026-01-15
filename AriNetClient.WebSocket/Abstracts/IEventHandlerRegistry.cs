using AriNetClient.WebSockets.Events;

namespace AriNetClient.WebSocket
{
    /// <summary>
    /// واجهة سجل معالجات الأحداث الذكي
    /// يدعم التسجيل التلقائي والاكتشاف
    /// </summary>
    public interface IEventHandlerRegistry
    {
        /// <summary>
        /// تسجيل معالج تلقائياً
        /// </summary>
        void RegisterHandler<THandler>() where THandler : class;

        /// <summary>
        /// تسجيل معالج بواسطة النوع
        /// </summary>
        void RegisterHandler(Type handlerType);

        /// <summary>
        /// تسجيل معالج عام
        /// </summary>
        void RegisterGlobalHandler(IGlobalEventHandler handler);

        /// <summary>
        /// إلغاء تسجيل معالج عام
        /// </summary>
        void UnregisterGlobalHandler(IGlobalEventHandler handler);

        /// <summary>
        /// الحصول على جميع المعالجات لحدث معين
        /// </summary>
        IReadOnlyList<IEventHandler<TEvent>> GetHandlers<TEvent>() where TEvent : BaseEvent;

        /// <summary>
        /// الحصول على جميع المعالجات العامة
        /// </summary>
        IReadOnlyList<IGlobalEventHandler> GetGlobalHandlers();

        /// <summary>
        /// الحصول على جميع المعالجات لأي حدث
        /// </summary>
        IReadOnlyList<IAutoEventHandler> GetHandlersForEvent(BaseEvent @event);

        /// <summary>
        /// التحقق مما إذا كان هناك معالج مسجل لحدث معين
        /// </summary>
        bool HasHandlers<TEvent>() where TEvent : BaseEvent;

        /// <summary>
        /// مسح جميع المعالجات المسجلة
        /// </summary>
        void Clear();

        /// <summary>
        /// الحصول على عدد جميع المعالجات
        /// </summary>
        int GetTotalHandlerCount();
    }
}
