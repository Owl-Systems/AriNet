using Newtonsoft.Json;

namespace AriNetClient.Dash.Events.Recording
{
    public class RecordingData
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("format")]
        public string Format { get; set; }

        [JsonProperty("target_uri")]
        public string TargetUri { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("duration")]
        public int? Duration { get; set; }

        [JsonProperty("talking_duration")]
        public int? TalkingDuration { get; set; }

        [JsonProperty("silence_duration")]
        public int? SilenceDuration { get; set; }
    }
}
