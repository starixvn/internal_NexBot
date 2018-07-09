namespace NexBot.Criteria.SkypeUser
{
    using Fanex.Data.Repository;

    public class InsertUserCriteria : CriteriaBase
    {
        public string SkypeDisplayName { get; set; }

        public string UserId { get; set; }

        public string ConversationId { get; set; }

        public override string GetSettingKey() => "Insert_SkypeUserWhenAddContact";

        public override bool IsValid() => !string.IsNullOrWhiteSpace(UserId) && !string.IsNullOrWhiteSpace(SkypeDisplayName);
    }
}