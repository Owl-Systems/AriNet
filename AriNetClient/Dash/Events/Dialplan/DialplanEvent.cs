using AriNetClient.Dash.Events;
using Newtonsoft.Json;

namespace AriNetClient.Dash.Events.Dialplan
{
    public class DialplanEvent : WazoEvent
    {
        [JsonProperty("dialplan_app")]
        public string DialplanApp { get; set; }

        [JsonProperty("dialplan_app_data")]
        public string DialplanAppData { get; set; }
    }
}
