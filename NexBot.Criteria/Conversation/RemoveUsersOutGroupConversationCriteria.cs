namespace NexBot.Criteria.Conversation
{
    using Fanex.Data.Repository;

    public class RemoveUsersOutGroupConversationCriteria : CriteriaBase
    {
        public string GroupConversationId { get; set; }

        public string UserConversationIds { get; set; }

        public override string GetSettingKey() => "GroupUserConversation_RemoveMulti";

        public override bool IsValid() => !string.IsNullOrWhiteSpace(GroupConversationId)
                                        && !string.IsNullOrWhiteSpace(UserConversationIds);
    }
}