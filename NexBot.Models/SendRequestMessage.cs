namespace NexBot.Models
{
    public class SendRequestMessage
    {
        public string ConversationId { get; set; }

        public long RequestId { get; set; }

        public string Message { get; set; }

        public string CallBackUrl { get; set; }
    }
}