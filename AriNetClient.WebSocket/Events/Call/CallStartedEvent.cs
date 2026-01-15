namespace AriNetClient.WebSockets.Events.Call
{
    /// <summary>
    /// حدث بدء المكالمة
    /// يتم إرساله عندما تبدأ مكالمة جديدة
    /// </summary>
    public class CallStartedEvent : BaseEvent
    {
        /// <summary>
        /// معرف المكالمة الفريد
        /// </summary>
        public string CallId => Data.GetValueOrDefault("call_id")?.ToString();

        /// <summary>
        /// رقم المتصل
        /// </summary>
        public string CallerNumber => Data.GetValueOrDefault("caller_number")?.ToString();

        /// <summary>
        /// رقم المستقبل
        /// </summary>
        public string CalleeNumber => Data.GetValueOrDefault("callee_number")?.ToString();

        /// <summary>
        /// معرف قناة المكالمة
        /// </summary>
        public string ChannelId => Data.GetValueOrDefault("channel_id")?.ToString();

        /// <summary>
        /// سياق المكالمة
        /// </summary>
        public string Context => Data.GetValueOrDefault("context")?.ToString();

        /// <summary>
        /// المعلومات الإضافية للمتصل
        /// </summary>
        public string CallerId => Data.GetValueOrDefault("caller_id")?.ToString();

        /// <summary>
        /// متغيرات المكالمة
        /// </summary>
        public Dictionary<string, string> CallVariables =>
            GetDataAs<Dictionary<string, string>>() ?? new();
    }

}
