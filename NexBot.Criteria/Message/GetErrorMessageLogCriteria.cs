namespace NexBot.Criteria.Message
{
    using Fanex.Data.Repository;

    public class GetErrorMessageLogCriteria : CriteriaBase
    {
        public override string GetSettingKey() => "ErrorMessageLog_Get";

        public override bool IsValid() => true;
    }
}
