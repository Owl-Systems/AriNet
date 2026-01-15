using AriNetClient.Dash.Events;
using Newtonsoft.Json;

namespace AriNetClient.Dash.Events.Bridges
{
    public abstract class BridgeEvent : WazoEvent
    {
        [JsonProperty("bridge")]
        public BridgeData Bridge { get; set; }
    }

    public class BridgeCreatedEvent : BridgeEvent { }

    public class BridgeDestroyedEvent : BridgeEvent { }

    public class BridgeMergedEvent : BridgeEvent { }
}
