using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AriNetClient.Dash.Events
{
    public abstract class WazoEvent
    {
        [JsonProperty("type")]
        public string EventType { get; set; }

        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonProperty("application")]
        public string Application { get; set; }

        [JsonExtensionData]
        public Dictionary<string, JToken> AdditionalData { get; set; }
    }

    // Event Categories
    public interface IChannelEvent { }
    public interface IBridgeEvent { }
    public interface IEndpointEvent { }
    public interface IPlaybackEvent { }
    public interface IRecordingEvent { }
    public interface IStasisEvent { }
    public interface IDeviceStateEvent { }
}
