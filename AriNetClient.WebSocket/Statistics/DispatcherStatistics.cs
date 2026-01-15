namespace AriNetClient.WebSocket.Statistics
{
    /// <summary>
    /// إحصائيات موزع الأحداث
    /// </summary>
    public class DispatcherStatistics
    {
        public int TotalTypedHandlers { get; set; }
        public int TotalGlobalHandlers { get; set; }
        public int TotalHandlers { get; set; }
    }


}
