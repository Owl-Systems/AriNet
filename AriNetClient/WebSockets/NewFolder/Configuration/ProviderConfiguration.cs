namespace AriNetClient.WebSockets.NewFolder.Configuration
{
    /// <summary>
    /// تكوين خاص بمزود معين
    /// </summary>
    public class ProviderConfiguration
    {
        public string ServerUrl { get; set; }
        public string AuthToken { get; set; }
        public string ApplicationName { get; set; }
        public Dictionary<string, string> CustomHeaders { get; set; } = new();
    }
}
