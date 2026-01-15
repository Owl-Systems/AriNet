using Newtonsoft.Json;

namespace AriNetClient.Dash.Events.Channels.Helpers
{
    public class ChannelData
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("caller")]
        public CallerId Caller { get; set; }

        [JsonProperty("connected")]
        public CallerId Connected { get; set; }

        [JsonProperty("accountcode")]
        public string AccountCode { get; set; }

        [JsonProperty("creationtime")]
        public DateTime CreationTime { get; set; }
    }
}
