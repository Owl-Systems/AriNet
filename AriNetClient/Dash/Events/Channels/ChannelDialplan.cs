using AriNetClient.Dash.Events;
using AriNetClient.Dash.Events.Channels.Helpers;
using Newtonsoft.Json;

namespace AriNetClient.Dash.Events.Channels
{
    public class ChannelDialplan : WazoEvent, IChannelEvent
    {
        [JsonProperty("channel")]
        public ChannelData Channel { get; set; }

        [JsonProperty("dialplan_app")]
        public string DialplanApp { get; set; }

        [JsonProperty("dialplan_app_data")]
        public string DialplanAppData { get; set; }
    }

}
