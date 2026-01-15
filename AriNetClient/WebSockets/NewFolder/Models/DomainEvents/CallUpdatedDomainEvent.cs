namespace AriNetClient.WebSockets.NewFolder.Models.DomainEvents
{
    /// <summary>
    /// حدث تحديث المكالمة في نطاقنا
    /// </summary>
    public class CallUpdatedDomainEvent : DomainEvent
    {
        public override string EventType => "call.updated";

        public string CallId { get; }
        public CallState PreviousState { get; }
        public CallState NewState { get; }
        public TimeSpan Duration { get; }
        public bool IsAnswered { get; }

        public CallUpdatedDomainEvent(
            string callId,
            CallState previousState,
            CallState newState,
            TimeSpan duration,
            bool isAnswered)
        {
            CallId = callId ?? throw new ArgumentNullException(nameof(callId));
            PreviousState = previousState;
            NewState = newState;
            Duration = duration;
            IsAnswered = isAnswered;
        }
    }
}
