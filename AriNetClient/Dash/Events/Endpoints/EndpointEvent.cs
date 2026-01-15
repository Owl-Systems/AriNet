using AriNetClient.Dash.Events;
using Newtonsoft.Json;

namespace AriNetClient.Dash.Events.Endpoints
{
    public abstract class EndpointEvent : WazoEvent
    {
        [JsonProperty("endpoint")]
        public EndpointData Endpoint { get; set; }
    }
}
