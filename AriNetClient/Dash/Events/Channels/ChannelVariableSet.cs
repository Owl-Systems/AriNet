using AriNetClient.Dash.Events;
using AriNetClient.Dash.Events.Channels.Helpers;
using Newtonsoft.Json;

namespace AriNetClient.Dash.Events.Channels
{
    public class ChannelVariableSet : WazoEvent, IChannelEvent
    {
        [JsonProperty("channel")]
        public ChannelData Channel { get; set; }

        [JsonProperty("variable")]
        public string Variable { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }
    }

}
