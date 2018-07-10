namespace NexBot.Models
{
    public class Conversation
    {
        public string ConversationId { get; set; }

        public string ChannelId { get; set; }

        public string ServiceUrl { get; set; }

        public bool IsGroup { get; set; }

        public bool IsActive { get; set; }

        public bool IsAdmin { get; set; }
    }
}
