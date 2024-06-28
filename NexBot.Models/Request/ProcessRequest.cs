namespace NexBot.Models.Request
{
    public class ProcessRequest
    {
        public long RequestId { get; set; }
        public string RequestIds { get; set; }

        public int ProcessedBy { get; set; }

        public string Command { get; set; }

        public string CallBackUrl { get; set; }
    }
}