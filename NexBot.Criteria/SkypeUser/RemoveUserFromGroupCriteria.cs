namespace NexBot.Criteria.SkypeUser
{
    using Fanex.Data.Repository;

    public class RemoveUserFromGroupCriteria : CriteriaBase
    {
        public string UserConversationId { get; set; }

        public string GroupConversationId { get; set; }

        public override string GetSettingKey() => "SkypeUser_RemoveUserFromGroup";

        public override bool IsValid() => !string.IsNullOrWhiteSpace(UserConversationId) && !string.IsNullOrWhiteSpace(GroupConversationId);
    }
}