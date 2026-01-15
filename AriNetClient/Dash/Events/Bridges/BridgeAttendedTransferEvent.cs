using AriNetClient.Dash.Events.Channels.Helpers;
using Newtonsoft.Json;

namespace AriNetClient.Dash.Events.Bridges
{
    public class BridgeAttendedTransferEvent : BridgeEvent
    {
        [JsonProperty("transfer_target")]
        public ChannelData TransferTarget { get; set; }

        [JsonProperty("transfer_target_channel")]
        public ChannelData TransferTargetChannel { get; set; }

        [JsonProperty("destination_link_first_leg")]
        public ChannelData DestinationLinkFirstLeg { get; set; }

        [JsonProperty("destination_link_second_leg")]
        public ChannelData DestinationLinkSecondLeg { get; set; }

        [JsonProperty("destination_type")]
        public string DestinationType { get; set; }

        [JsonProperty("destination_application")]
        public string DestinationApplication { get; set; }

        [JsonProperty("transfer_target_creation_time")]
        public DateTime TransferTargetCreationTime { get; set; }

        [JsonProperty("transfer_target_creator")]
        public string TransferTargetCreator { get; set; }
    }
}
