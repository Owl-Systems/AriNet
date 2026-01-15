namespace AriNetClient.WebSockets.NewFolder.Models.DomainEvents
{
    /// <summary>
    /// الحدث الأساسي لأحداث النطاق
    /// لا يعرف أي شيء عن الخادم أو WebSocket
    /// </summary>
    public abstract class DomainEvent
    {
        /// <summary>
        /// معرف الحدث الفريد (يتم توليده محليًا)
        /// </summary>
        public Guid EventId { get; } = Guid.NewGuid();

        /// <summary>
        /// وقت حدوث الحدث في نظامنا (ليس وقت الخادم)
        /// </summary>
        public DateTime OccurredOn { get; } = DateTime.UtcNow();

        /// <summary>
        /// نوع الحدث في نطاقنا (ليس نوع حدث الخادم)
        /// </summary>
        public abstract string EventType { get; }
    }
    /// <summary>
    /// حدث بدء المكالمة في نطاقنا
    /// لا يعرف أي شيء عن Wazo أو أي خادم آخر
    /// </summary>
    public class CallStartedDomainEvent : DomainEvent
    {
        public override string EventType => "call.started";

        // خصائص قوية النوع، محددة بوضوح
        public string CallId { get; }
        public PhoneNumber Caller { get; }
        public PhoneNumber Callee { get; }
        public DateTime CallStartTime { get; }
        public string Context { get; }

        // ✅ لا يوجد Dictionary
        // ✅ لا توجد تسميات خادم
        // ✅ جميع الخصائص إلزامية (لا تسمح بقيم null)

        public CallStartedDomainEvent(
            string callId,
            PhoneNumber caller,
            PhoneNumber callee,
            DateTime callStartTime,
            string context)
        {
            CallId = callId ?? throw new ArgumentNullException(nameof(callId));
            Caller = caller ?? throw new ArgumentNullException(nameof(caller));
            Callee = callee ?? throw new ArgumentNullException(nameof(callee));
            CallStartTime = callStartTime;
            Context = context ?? throw new ArgumentNullException(nameof(context));
        }
    }
}
