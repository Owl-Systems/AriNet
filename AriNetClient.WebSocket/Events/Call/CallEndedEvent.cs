namespace AriNetClient.WebSockets.Events.Call
{
    /// <summary>
    /// حدث انتهاء المكالمة
    /// يتم إرساله عندما تنتهي المكالمة
    /// </summary>
    public class CallEndedEvent : BaseEvent
    {
        /// <summary>
        /// معرف المكالمة الفريد
        /// </summary>
        public string CallId => Data.GetValueOrDefault("call_id")?.ToString();

        /// <summary>
        /// سبب انتهاء المكالمة
        /// </summary>
        public string HangupCause => Data.GetValueOrDefault("hangup_cause")?.ToString();

        /// <summary>
        /// المدة الكلية للمكالمة بالثواني
        /// </summary>
        public int TotalDuration => int.TryParse(Data.GetValueOrDefault("total_duration")?.ToString(), out var duration)
            ? duration
            : 0;

        /// <summary>
        /// مدة الحديث الفعلية بالثواني
        /// </summary>
        public int TalkDuration => int.TryParse(Data.GetValueOrDefault("talk_duration")?.ToString(), out var duration)
            ? duration
            : 0;

        /// <summary>
        /// تاريخ ووقت بدء المكالمة
        /// </summary>
        public DateTime StartTime => DateTime.TryParse(Data.GetValueOrDefault("start_time")?.ToString(), out var date)
            ? date
            : DateTime.MinValue;

        /// <summary>
        /// تاريخ ووقت انتهاء المكالمة
        /// </summary>
        public DateTime EndTime => DateTime.TryParse(Data.GetValueOrDefault("end_time")?.ToString(), out var date)
            ? date
            : DateTime.MinValue;

        /// <summary>
        /// هل المكالمة سجلت؟
        /// </summary>
        public bool IsRecorded => Data.GetValueOrDefault("recorded")?.ToString() == "true";

        /// <summary>
        /// المسار الملف المسجل
        /// </summary>
        public string RecordingPath => Data.GetValueOrDefault("recording_path")?.ToString();
    }


}
