//using AriNetClient.Dash.Events;
//using AriNetClient.Dash.Events.Channels;
//using AriNetClient.Dash.Events.Stasis;
//using Newtonsoft.Json;
//using Newtonsoft.Json.Linq;

//namespace AriNetClient.Dash.Events.Serialization
//{

//    //public class WazoEventConverter : JsonConverter
//    //{
//    //    private static readonly Dictionary<string, Type> EventTypeMap = new Dictionary<string, Type>
//    //    {
//    //        // Channel Events
//    //        { "ChannelCreated", typeof(ChannelCreatedEvent) },
//    //        { "ChannelDestroyed", typeof(ChannelDestroyedEvent) },
//    //        { "ChannelStateChange", typeof(ChannelStateChangeEvent) },
//    //        { "ChannelDialplan", typeof(ChannelDialplanEvent) },
//    //        { "ChannelCallerId", typeof(ChannelCallerIdEvent) },

//    //        // Bridge Events
//    //        { "BridgeCreated", typeof(BridgeCreatedEvent) },
//    //        { "BridgeDestroyed", typeof(BridgeDestroyedEvent) },
//    //        { "BridgeMerged", typeof(BridgeMergedEvent) },
//    //        { "BridgeBlindTransfer", typeof(BridgeBlindTransferEvent) },
//    //        { "BridgeAttendedTransfer", typeof(BridgeAttendedTransferEvent) },

//    //        // Endpoint Events
//    //        { "EndpointStateChange", typeof(EndpointStateChangeEvent) },

//    //        // Device State Events
//    //        { "DeviceStateChanged", typeof(DeviceStateChangedEvent) },

//    //        // Dialplan Events
//    //        { "Dialplan", typeof(DialplanEvent) },

//    //        // Playback Events
//    //        { "PlaybackStarted", typeof(PlaybackStartedEvent) },
//    //        { "PlaybackFinished", typeof(PlaybackFinishedEvent) },

//    //        // Recording Events
//    //        { "RecordingStarted", typeof(RecordingStartedEvent) },
//    //        { "RecordingFinished", typeof(RecordingFinishedEvent) },
//    //        { "RecordingFailed", typeof(RecordingFailedEvent) },

//    //        ////  Events
//    //        //{ "Start", typeof(StartEvent) },
//    //        //{ "End", typeof(EndEvent) },
//    //    };

//    //    public override bool CanConvert(Type objectType)
//    //    {
//    //        return objectType == typeof(WazoEvent);
//    //    }

//    //    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
//    //    {
//    //        JObject obj = JObject.Load(reader);

//    //        string type = obj["type"]?.Value<string>();

//    //        if (string.IsNullOrEmpty(type))
//    //        {
//    //            throw new JsonSerializationException("Event type is missing");
//    //        }

//    //        if (EventTypeMap.TryGetValue(type, out Type eventType))
//    //        {
//    //            WazoEvent eventInstance = (WazoEvent)Activator.CreateInstance(eventType);
//    //            serializer.Populate(obj.CreateReader(), eventInstance);
//    //            return eventInstance;
//    //        }

//    //        // إذا لم يكن النوع معروفاً، نرجع فئة أساسية
//    //        var unknownEvent = new UnknownEvent();
//    //        serializer.Populate(obj.CreateReader(), unknownEvent);
//    //        return unknownEvent;
//    //    }

//    //    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
//    //    {
//    //        throw new NotImplementedException();
//    //    }
//    //}

//    public class WazoEventConverter : JsonConverter
//    {
//        private static readonly Dictionary<string, Type> _eventTypeMap = new Dictionary<string, Type>
//        {
//            // Channel Events
//            { "ChannelCreated", typeof(ChannelCreated) },
//            { "ChannelDestroyed", typeof(ChannelDestroyed) },
//            { "ChannelStateChange", typeof(ChannelStateChanged) },
//            { "ChannelDialplan", typeof(ChannelDialplan) },
//            { "ChannelCallerId", typeof(ChannelCallerId) },
//            { "ChannelHangupRequest", typeof(ChannelHangupRequest) },
//            { "ChannelVarset", typeof(ChannelVariableSet) },

//            // Bridge Events
//            { "BridgeCreated", typeof(BridgeCreated) },
//            { "BridgeDestroyed", typeof(BridgeDestroyed) },
//            { "BridgeMerged", typeof(BridgeMerged) },
//            { "BridgeBlindTransfer", typeof(BlindTransfer) },
//            { "BridgeAttendedTransfer", typeof(AttendedTransfer) },

//            // Stasis Events
//            { "StasisStart", typeof(StasisStart) },
//            { "StasisEnd", typeof(StasisEnd) },

//            // Playback Events
//            { "PlaybackStarted", typeof(PlaybackStarted) },
//            { "PlaybackFinished", typeof(PlaybackFinished) },

//            // Recording Events
//            { "RecordingStarted", typeof(RecordingStarted) },
//            { "RecordingFinished", typeof(RecordingFinished) },
//            { "RecordingFailed", typeof(RecordingFailed) },

//            // Endpoint Events
//            { "EndpointStateChange", typeof(EndpointStateChanged) },

//            // Device State Events
//            { "DeviceStateChanged", typeof(DeviceStateChanged) }
//        };

//        public override bool CanConvert(Type objectType)
//        {
//            return objectType == typeof(WazoEvent);
//        }

//        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
//        {
//            var jObject = JObject.Load(reader);
//            var eventType = jObject["type"]?.Value<string>();

//            if (string.IsNullOrEmpty(eventType))
//            {
//                return new UnknownEvent { RawData = jObject };
//            }

//            if (_eventTypeMap.TryGetValue(eventType, out Type targetType))
//            {
//                var instance = (WazoEvent)Activator.CreateInstance(targetType);
//                serializer.Populate(jObject.CreateReader(), instance);
//                return instance;
//            }

//            return new UnknownEvent
//            {
//                EventType = eventType,
//                RawData = jObject
//            };
//        }

//        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
//        {
//            throw new NotImplementedException();
//        }
//    }

//    public class UnknownEvent : WazoEvent
//    {
//        public JObject RawData { get; set; }
//    }
//}
