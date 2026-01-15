using AriNetClient.Dash.Events;
using AriNetClient.Dash.Events.Channels.Helpers;
using Newtonsoft.Json;

namespace AriNetClient.Dash.Events.Channels
{
    public class ChannelCreated : WazoEvent, IChannelEvent
    {
        [JsonProperty("channel")]
        public ChannelData Channel { get; set; }
    }
}
