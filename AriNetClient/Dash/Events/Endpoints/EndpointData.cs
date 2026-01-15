using Newtonsoft.Json;

namespace AriNetClient.Dash.Events.Endpoints
{
    public class EndpointData
    {
        [JsonProperty("technology")]
        public string Technology { get; set; }

        [JsonProperty("resource")]
        public string Resource { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("channel_ids")]
        public List<string> ChannelIds { get; set; }
    }
}
