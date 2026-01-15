using AriNetClient.Dash.Events;
using AriNetClient.Dash.Events.Channels.Helpers;
using Newtonsoft.Json;

namespace AriNetClient.Dash.Events.Stasis
{
    public class StasisEnd : WazoEvent, IStasisEvent
    {
        [JsonProperty("channel")]
        public ChannelData Channel { get; set; }

        [JsonProperty("args")]
        public List<string> Args { get; set; }
    }

}
