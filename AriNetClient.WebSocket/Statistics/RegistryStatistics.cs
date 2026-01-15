namespace AriNetClient.WebSocket.Statistics
{
    /// <summary>
    /// إحصائيات سجل المعالجات
    /// </summary>
    public class RegistryStatistics
    {
        public int TotalGlobalHandlers { get; set; }
        public int TotalAutoHandlers { get; set; }
        public int TotalTypedHandlers { get; set; }
        public List<string> RegisteredEventTypes { get; set; } = new();
        public int TotalHandlers { get; set; }

        public override string ToString()
        {
            return $"Global: {TotalGlobalHandlers}, Auto: {TotalAutoHandlers}, Typed: {TotalTypedHandlers}, Total: {TotalHandlers}";
        }
    }
}
