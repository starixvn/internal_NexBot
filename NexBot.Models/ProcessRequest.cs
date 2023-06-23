namespace NexBot.Models
{
    public class ProcessRequest
    {
        public long RequestId { get; set; }

        public int ProcessedBy { get; set; }

        public string NickName { get; set; }

        public string Command { get; set; }

        public string CallBackUrl { get; set; }
    }
}
