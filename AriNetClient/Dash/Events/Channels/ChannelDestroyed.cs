using AriNetClient.Dash.Events;
using AriNetClient.Dash.Events.Channels.Helpers;
using Newtonsoft.Json;

namespace AriNetClient.Dash.Events.Channels
{
    public class ChannelDestroyed : WazoEvent, IChannelEvent
    {
        [JsonProperty("channel")]
        public ChannelData Channel { get; set; }

        [JsonProperty("cause")]
        public int Cause { get; set; }

        [JsonProperty("cause_txt")]
        public string CauseText { get; set; }
    }

}
