namespace NexBot.Criteria.Conversation
{
    using NexBot.Core.Models;

    public class RemoveConversationByIdCommand : BaseCommand
    {
        public string ConversationId { get; set; }

        public override string GetSettingKey() => "Conversation_DeactiveById";

        public override bool IsValid() => !string.IsNullOrWhiteSpace(ConversationId);
    }
}