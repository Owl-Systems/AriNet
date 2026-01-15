namespace AriNetClient.Models.Calls
{
    public class OriginateRequest
    {
        public string Extension { get; set; }
        public string Context { get; set; } = "default";
        public int Priority { get; set; } = 1;
        public int Timeout { get; set; } = 30;
        public string CallerId { get; set; }
        public Dictionary<string, string> Variables { get; set; }
        public string Application { get; set; }
        public string Endpoint { get; set; }
    }
}
