using AriNetClient.WebSockets.Events;

namespace AriNetClient.WebSocket
{
    /// <summary>
    /// واجهة للمعالجات التي تدعم الاكتشاف التلقائي
    /// </summary>
    public interface IAutoEventHandler : IEventHandler<BaseEvent>
    {
        /// <summary>
        /// الحصول على أنواع الأحداث التي يعالجها هذا المعالج
        /// </summary>
        IEnumerable<Type> GetHandledEventTypes();

        /// <summary>
        /// التحقق مما إذا كان المعالج يدعم نوع حدث معين
        /// </summary>
        bool CanHandle(string eventType);
    }
}
