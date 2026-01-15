using AriNetClient.WebSockets.Events;

namespace AriNetClient.WebSockets.Abstracts
{
    /// <summary>
    /// واجهة معالج الأحداث العامة (لجميع الأحداث)
    /// </summary>
    public interface IGlobalEventHandler : IEventHandler<BaseEvent>
    {
        /// <summary>
        /// التحقق مما إذا كان المعالج يدعم نوع حدث معين
        /// </summary>
        bool CanHandle(string eventType);
    }
}
