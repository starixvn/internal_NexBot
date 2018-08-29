namespace NexBot.Models
{
    public class ConversationModel
    {
        public string ConversationId { get; set; }

        public string UserName { get; set; }

        public string NickName { get; set; }

        public string ChannelId { get; set; }

        public string ServiceUrl { get; set; }

        public bool IsGroup { get; set; }

        public bool IsActive { get; set; }

        public bool IsAdmin { get; set; }

        public string ConversationBotId { get; set; }

        public string ConversationBotName { get; set; }
    }
}