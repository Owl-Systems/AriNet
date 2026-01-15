using AriNetClient.Dash.Events;
using Newtonsoft.Json;

namespace AriNetClient.Dash.Events.DeviceState
{
    public class DeviceStateChangedEvent : WazoEvent
    {
        [JsonProperty("device_state")]
        public DeviceStateData DeviceState { get; set; }
    }
}
