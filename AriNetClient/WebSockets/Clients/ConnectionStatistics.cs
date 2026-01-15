namespace AriNetClient.WebSockets.Clients
{
    /// <summary>
    /// إحصائيات الاتصال
    /// </summary>
    public class ConnectionStatistics
    {
        public bool IsConnected { get; set; }
        public bool IsInitialized { get; set; }
        public bool IsSubscribed { get; set; }
        public int TotalHandlers { get; set; }
        public TimeSpan Uptime { get; set; }
        public int ReconnectionAttempts { get; set; }
    }
}
