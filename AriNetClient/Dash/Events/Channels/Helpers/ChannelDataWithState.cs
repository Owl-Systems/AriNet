using Newtonsoft.Json;

namespace AriNetClient.Dash.Events.Channels.Helpers
{
    public class ChannelDataWithState : ChannelData
    {
        [JsonProperty("state")]
        public new string State { get; set; }
    }
}
