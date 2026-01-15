using Newtonsoft.Json;

namespace AriNetClient.Dash.Events.Bridges
{
    public class BridgeData
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("technology")]
        public string Technology { get; set; }

        [JsonProperty("bridge_type")]
        public string BridgeType { get; set; }

        [JsonProperty("bridge_class")]
        public string BridgeClass { get; set; }

        [JsonProperty("creator")]
        public string Creator { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("channels")]
        public List<string> Channels { get; set; }

        [JsonProperty("creationtime")]
        public DateTime CreationTime { get; set; }
    }
}
