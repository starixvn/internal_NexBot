namespace NexBot.Criteria.Conversation
{
    using NexBot.Core.Models;

    public class InsertConversationCommand : BaseCommand
    {
        public string ConversationId { get; set; }

        public string ChannelId { get; set; }

        public string ServiceUrl { get; set; }

        public bool? IsGroup { get; set; }

        public string UserName { get; set; }

        public string FirstName { get; set; }

        public bool IsActive { get; set; }

        public bool IsAdmin { get; set; }

        public string ConversationBotId { get; set; }

        public string ConversationBotName { get; set; }

        public override string GetSettingKey() => "Conversation_Insert";

        public override bool IsValid() => !string.IsNullOrWhiteSpace(ConversationId)
            && !string.IsNullOrWhiteSpace(ChannelId)
            && !string.IsNullOrWhiteSpace(ServiceUrl);
    }
}