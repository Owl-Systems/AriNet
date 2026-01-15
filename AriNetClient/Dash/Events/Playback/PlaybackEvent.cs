using AriNetClient.Dash.Events;
using Newtonsoft.Json;

namespace AriNetClient.Dash.Events.Playback
{
    public abstract class PlaybackEvent : WazoEvent
    {
        [JsonProperty("playback")]
        public PlaybackData Playback { get; set; }
    }

    public class PlaybackStartedEvent : PlaybackEvent { }

    public class PlaybackFinishedEvent : PlaybackEvent { }
}
