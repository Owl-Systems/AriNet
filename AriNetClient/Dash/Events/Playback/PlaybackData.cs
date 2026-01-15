using Newtonsoft.Json;

namespace AriNetClient.Dash.Events.Playback
{
    public class PlaybackData
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("media_uri")]
        public string MediaUri { get; set; }

        [JsonProperty("target_uri")]
        public string TargetUri { get; set; }

        [JsonProperty("language")]
        public string Language { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }
    }
}
