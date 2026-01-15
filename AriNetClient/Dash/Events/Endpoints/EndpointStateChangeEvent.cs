using Newtonsoft.Json;

namespace AriNetClient.Dash.Events.Endpoints
{
    public class EndpointStateChangeEvent : EndpointEvent
    {
        [JsonProperty("endpoint")]
        public new EndpointDataWithState Endpoint { get; set; }
    }
}
