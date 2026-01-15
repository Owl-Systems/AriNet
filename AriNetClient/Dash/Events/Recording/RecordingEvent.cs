using AriNetClient.Dash.Events;
using Newtonsoft.Json;

namespace AriNetClient.Dash.Events.Recording
{
    public abstract class RecordingEvent : WazoEvent
    {
        [JsonProperty("recording")]
        public RecordingData Recording { get; set; }
    }

    public class RecordingStartedEvent : RecordingEvent { }

    public class RecordingFinishedEvent : RecordingEvent
    {
        [JsonProperty("duration")]
        public int Duration { get; set; }
    }

    public class RecordingFailedEvent : RecordingEvent { }
}
