namespace NexBot.Criteria.SkypeUser
{
    using Fanex.Data.Repository;

    public class DeactiveUserCriteria : CriteriaBase
    {
        public string UserId { get; set; }

        public override string GetSettingKey() => "Update_DeactiveUser";

        public override bool IsValid() => !string.IsNullOrWhiteSpace(UserId);
    }
}