namespace NexBot.Criteria.SkypeUser
{
    using Fanex.Data.Repository;

    public class AddNewUserToGroupCriteria : CriteriaBase
    {
        public string GroupConversationId { get; set; }

        public string UserConversationId { get; set; }

        public override string GetSettingKey() => "SkypeGroup_AddNewMemberToGroup";

        public override bool IsValid() => !string.IsNullOrWhiteSpace(GroupConversationId)
                                         && !string.IsNullOrWhiteSpace(UserConversationId);
    }
}