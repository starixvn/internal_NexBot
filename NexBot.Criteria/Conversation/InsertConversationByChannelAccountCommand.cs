namespace NexBot.Criteria.Conversation
{
    using NexBot.Core.Models;

    public class InsertConversationByChannelAccountCommand : BaseCommand
    {
        public string ConversationId { get; set; }

        public string UserName { get; set; }

        public override string GetSettingKey() => "Conversation_InsertByChannelAccount";

        public override bool IsValid() => !string.IsNullOrWhiteSpace(ConversationId);
    }
}