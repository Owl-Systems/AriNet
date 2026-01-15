using AriNetClient.WebSockets.Events;
using AriNetClient.WebSockets.Events.Call;

namespace AriNetClient.WebSocket.Mappers
{
    /// <summary>
    /// مخصص لتعيين أنواع الأحداث بين تسميات Wazo وأنواع .NET
    /// </summary>
    public static class EventTypeMapper
    {
        private static readonly Dictionary<string, Type> _eventTypeMappings = new(StringComparer.OrdinalIgnoreCase)
        {
            ["started"] = typeof(CallStartedEvent),
            ["updated"] = typeof(CallUpdatedEvent),
            ["ended"] = typeof(CallEndedEvent),
            ["call_started"] = typeof(CallStartedEvent),
            ["call_updated"] = typeof(CallUpdatedEvent),
            ["call_ended"] = typeof(CallEndedEvent),
        };

        private static readonly Dictionary<Type, string> _reverseMappings = new();

        static EventTypeMapper()
        {
            // إنشاء التعيين العكسي
            foreach (var kvp in _eventTypeMappings)
            {
                if (!_reverseMappings.ContainsKey(kvp.Value))
                {
                    _reverseMappings[kvp.Value] = kvp.Key;
                }
            }
        }

        /// <summary>
        /// الحصول على نوع .NET من اسم الحدث
        /// </summary>
        public static Type MapToDotNetType(string eventType)
        {
            return _eventTypeMappings.TryGetValue(eventType, out var dotNetType)
                ? dotNetType
                : typeof(BaseEvent);
        }

        /// <summary>
        /// الحصول على اسم الحدث من نوع .NET
        /// </summary>
        public static string MapToEventType<TEvent>() where TEvent : BaseEvent
        {
            return _reverseMappings.TryGetValue(typeof(TEvent), out var eventType)
                ? eventType
                : typeof(TEvent).Name.ToLower();
        }

        /// <summary>
        /// تسجيل تعيين جديد لنوع حدث
        /// </summary>
        public static void RegisterMapping(string eventType, Type dotNetType)
        {
            if (!typeof(BaseEvent).IsAssignableFrom(dotNetType))
            {
                throw new ArgumentException($"Type must inherit from BaseEvent", nameof(dotNetType));
            }

            _eventTypeMappings[eventType] = dotNetType;
            _reverseMappings[dotNetType] = eventType;
        }

        /// <summary>
        /// التحقق مما إذا كان هناك تعيين لنوع حدث معين
        /// </summary>
        public static bool HasMapping(string eventType)
        {
            return _eventTypeMappings.ContainsKey(eventType);
        }

        /// <summary>
        /// الحصول على جميع التعيينات
        /// </summary>
        public static IReadOnlyDictionary<string, Type> GetAllMappings()
        {
            return _eventTypeMappings;
        }
    }
}
