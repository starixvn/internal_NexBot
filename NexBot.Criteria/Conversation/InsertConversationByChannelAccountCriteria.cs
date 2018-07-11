namespace NexBot.Criteria.Conversation
{
    using Fanex.Data.Repository;

    public class InsertConversationByChannelAccountCriteria : CriteriaBase
    {
        public string ConversationId { get; set; }

        public string UserName { get; set; }

        public override string GetSettingKey() => "Conversation_InsertByChannelAccount";

        public override bool IsValid() => !string.IsNullOrWhiteSpace(ConversationId);
    }
}