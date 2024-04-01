namespace NexBot.Models
{
    public class Bot
    {
        public int BotId { get; set; }

        public string ConversationId { get; set; }

        public string BotName { get; set; }

        public int ChannelId { get; set; }

        public string ChannelName { get; set; }

        public string ServiceUrl { get; set; }

        public int AppId { get; set; }

        public string MicrosoftAppId { get; set; }
    }
}
