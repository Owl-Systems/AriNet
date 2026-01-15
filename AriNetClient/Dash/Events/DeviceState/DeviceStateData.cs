using Newtonsoft.Json;

namespace AriNetClient.Dash.Events.DeviceState
{
    public class DeviceStateData
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }
    }
}
