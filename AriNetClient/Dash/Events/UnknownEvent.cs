using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AriNetClient.Dash.Events
{
    public class UnknownEvent : WazoEvent
    {
        [JsonProperty("raw_data")]
        public JObject RawData { get; set; }
    }
}
