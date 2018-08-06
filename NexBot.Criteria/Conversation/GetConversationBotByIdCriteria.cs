namespace NexBot.Criteria.Conversation
{
    using Fanex.Data.Repository;

    public class GetConversationBotByIdCriteria : CriteriaBase
    {
        public string ConversationId { get; set; }

        public override string GetSettingKey() => "ConversationBot_GetById";

        public override bool IsValid() => !string.IsNullOrWhiteSpace(ConversationId);
    }
}
