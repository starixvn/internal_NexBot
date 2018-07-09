namespace NexBot.Criteria.SkypeUser
{
    using Fanex.Data.Repository;

    public class DeactiveUserCriteria : CriteriaBase
    {
        public string UserConversationId { get; set; }

        public override string GetSettingKey() => "SkypeUser_DeactiveUser";

        public override bool IsValid() => !string.IsNullOrWhiteSpace(UserConversationId);
    }
}