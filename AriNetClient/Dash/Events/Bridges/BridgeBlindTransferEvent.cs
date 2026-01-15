using AriNetClient.Dash.Events.Channels.Helpers;
using Newtonsoft.Json;

namespace AriNetClient.Dash.Events.Bridges
{
    public class BridgeBlindTransferEvent : BridgeEvent
    {
        [JsonProperty("channel")]
        public ChannelData Channel { get; set; }

        [JsonProperty("replace_channel")]
        public ChannelData ReplaceChannel { get; set; }

        [JsonProperty("transferee")]
        public ChannelData Transferee { get; set; }

        [JsonProperty("exten")]
        public string Exten { get; set; }

        [JsonProperty("context")]
        public string Context { get; set; }

        [JsonProperty("result")]
        public string Result { get; set; }
    }
}
