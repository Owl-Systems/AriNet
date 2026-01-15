namespace AriNetClient.WebSockets.Events.Call
{
    /// <summary>
    /// حدث تحديث المكالمة
    /// يتم إرساله عندما تتغير حالة المكالمة
    /// </summary>
    public class CallUpdatedEvent : BaseEvent
    {
        /// <summary>
        /// معرف المكالمة الفريد
        /// </summary>
        public string CallId => Data.GetValueOrDefault("call_id")?.ToString();

        /// <summary>
        /// الحالة الجديدة للمكالمة
        /// </summary>
        public string NewState => Data.GetValueOrDefault("new_state")?.ToString();

        /// <summary>
        /// الحالة السابقة للمكالمة
        /// </summary>
        public string PreviousState => Data.GetValueOrDefault("previous_state")?.ToString();

        /// <summary>
        /// مدة المكالمة حتى الآن بالثواني
        /// </summary>
        public int Duration => int.TryParse(Data.GetValueOrDefault("duration")?.ToString(), out var duration)
            ? duration
            : 0;

        /// <summary>
        /// هل المكالمة تم الرد عليها؟
        /// </summary>
        public bool IsAnswered => Data.GetValueOrDefault("answered")?.ToString() == "true";

        /// <summary>
        /// سبب تغيير الحالة
        /// </summary>
        public string Reason => Data.GetValueOrDefault("reason")?.ToString();
    }


}
