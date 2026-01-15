namespace AriNetClient.WebSockets.NewFolder.Models.DomainEvents
{
    /// <summary>
    /// حدث انتهاء المكالمة في نطاقنا
    /// </summary>
    public class CallEndedDomainEvent : DomainEvent
    {
        public override string EventType => "call.ended";

        public string CallId { get; }
        public DateTime StartTime { get; }
        public DateTime EndTime { get; }
        public TimeSpan TotalDuration { get; }
        public TimeSpan TalkDuration { get; }
        public HangupCause HangupCause { get; }
        public bool WasRecorded { get; }

        public CallEndedDomainEvent(
            string callId,
            DateTime startTime,
            DateTime endTime,
            TimeSpan totalDuration,
            TimeSpan talkDuration,
            HangupCause hangupCause,
            bool wasRecorded)
        {
            CallId = callId ?? throw new ArgumentNullException(nameof(callId));
            StartTime = startTime;
            EndTime = endTime;
            TotalDuration = totalDuration;
            TalkDuration = talkDuration;
            HangupCause = hangupCause;
            WasRecorded = wasRecorded;
        }
    }
}
