using Newtonsoft.Json;

namespace AriNetClient.Dash.Events.Endpoints
{
    public class EndpointDataWithState : EndpointData
    {
        [JsonProperty("state")]
        public new string State { get; set; }
    }
}
