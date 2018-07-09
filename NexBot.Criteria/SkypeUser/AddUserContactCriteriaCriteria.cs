namespace NexBot.Criteria.SkypeUser
{
    using Fanex.Data.Repository;

    public class AddUserContactCriteriaCriteria : CriteriaBase
    {
        public string SkypeDisplayName { get; set; }

        public string ConversationId { get; set; }

        public override string GetSettingKey() => "SkypeUser_InsertWhenAddContact";

        public override bool IsValid() => !string.IsNullOrWhiteSpace(SkypeDisplayName);
    }
}