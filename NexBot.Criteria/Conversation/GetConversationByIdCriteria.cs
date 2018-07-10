namespace NexBot.Criteria.Conversation
{
    using Fanex.Data.Repository;

    public class GetConversationByIdCriteria : CriteriaBase
    {
        public string ConversationId { get; set; }

        public override string GetSettingKey() => "Conversation_GetById";

        public override bool IsValid() => !string.IsNullOrWhiteSpace(ConversationId);
    }
}
