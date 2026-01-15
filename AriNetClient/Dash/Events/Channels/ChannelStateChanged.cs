using AriNetClient.Dash.Events;
using AriNetClient.Dash.Events.Channels.Helpers;
using Newtonsoft.Json;

namespace AriNetClient.Dash.Events.Channels
{
    public class ChannelStateChanged : WazoEvent, IChannelEvent
    {
        [JsonProperty("channel")]
        public ChannelDataWithState Channel { get; set; }
    }

}
