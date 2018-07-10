namespace NexBot.Criteria.Conversation
{
    using Fanex.Data.Repository;

    public class InsertConversationCriteria : CriteriaBase
    {
        public string ConversationId { get; set; }

        public string ChannelId { get; set; }

        public string ServiceUrl { get; set; }

        public bool IsGroup { get; set; }

        public string UserName { get; set; }

        public bool IsActive { get; set; }

        public bool IsAdmin { get; set; }

        public override string GetSettingKey() => "Conversation_Insert";

        public override bool IsValid() => !string.IsNullOrWhiteSpace(ConversationId)
            && !string.IsNullOrWhiteSpace(ChannelId)
            && !string.IsNullOrWhiteSpace(ServiceUrl);
    }
}
