namespace NexBot.Models
{
    public class Message
    {
        public string ConversationId { get; set; }

        public string FromConversationId { get; set; }

        public string FromName { get; set; }

        public string MessageContent { get; set; }
    }
}
