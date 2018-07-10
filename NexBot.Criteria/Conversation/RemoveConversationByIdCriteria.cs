namespace NexBot.Criteria.Conversation
{
    using Fanex.Data.Repository;

    public class RemoveConversationByIdCriteria : CriteriaBase
    {
        public string ConversationId { get; set; }

        public override string GetSettingKey() => "Conversation_DeactiveById";

        public override bool IsValid() => !string.IsNullOrWhiteSpace(ConversationId);
    }
}