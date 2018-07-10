namespace NexBot.Criteria.Conversation
{
    using Fanex.Data.Repository;

    public class InsertGroupUserConversationCriteria : CriteriaBase
    {
        public string GroupConversationId { get; set; }

        public string UserConversationId { get; set; }

        public override string GetSettingKey() => "GroupUserConversation_Insert";

        public override bool IsValid() => !string.IsNullOrWhiteSpace(GroupConversationId)
                                        && !string.IsNullOrWhiteSpace(UserConversationId);
    }
}