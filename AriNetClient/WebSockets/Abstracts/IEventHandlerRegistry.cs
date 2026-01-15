using AriNetClient.WebSockets.Events;
using System.Collections.ObjectModel;

namespace AriNetClient.WebSockets.Abstracts
{
    /// <summary>
    /// واجهة سجل معالجات الأحداث
    /// </summary>
    public interface IEventHandlerRegistry
    {
        /// <summary>
        /// تسجيل معالج لحدث معين
        /// </summary>
        void RegisterHandler<TEvent, THandler>() where TEvent : BaseEvent where THandler : IEventHandler<TEvent>;

        /// <summary>
        /// إلغاء تسجيل معالج لحدث معين
        /// </summary>
        void UnregisterHandler<TEvent, THandler>() where TEvent : BaseEvent where THandler : IEventHandler<TEvent>;

        /// <summary>
        /// الحصول على جميع المعالجات لحدث معين
        /// </summary>
        ReadOnlyCollection<IEventHandler<TEvent>> GetHandlers<TEvent>() where TEvent : BaseEvent;

        /// <summary>
        /// تسجيل معالج عام لجميع الأحداث
        /// </summary>
        void RegisterGlobalHandler(IGlobalEventHandler handler);

        /// <summary>
        /// إلغاء تسجيل معالج عام
        /// </summary>
        void UnregisterGlobalHandler(IGlobalEventHandler handler);

        /// <summary>
        /// الحصول على جميع المعالجات العامة
        /// </summary>
        ReadOnlyCollection<IGlobalEventHandler> GetGlobalHandlers();

        /// <summary>
        /// مسح جميع المعالجات المسجلة
        /// </summary>
        void Clear();

        /// <summary>
        /// التحقق مما إذا كان هناك معالج مسجل لحدث معين
        /// </summary>
        bool HasHandlers<TEvent>() where TEvent : BaseEvent;
        int GetTotalHandlerCount();
    }
}
