namespace AriNetClient.Models.Calls
{
    public class Call
    {
        public string Id { get; set; }
        public string State { get; set; }
        public string CallerNumber { get; set; }
        public string CalleeNumber { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? AnswerTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int Duration { get; set; }
        public Dictionary<string, object> Variables { get; set; }
    }
}
