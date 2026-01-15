using Newtonsoft.Json;

namespace AriNetClient.Dash.Events.Channels.Helpers
{
    public class CallerId
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("number")]
        public string Number { get; set; }
    }
}
