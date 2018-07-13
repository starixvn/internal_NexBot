namespace NexBot.Criteria
{
    using Fanex.Data.Repository;

    public class InsertSystemLogCriteria : CriteriaBase
    {
        public string LogContent { get; set; }

        public override string GetSettingKey() => "SystemLog_Insert";

        public override bool IsValid() => !string.IsNullOrWhiteSpace(LogContent);
    }
}
