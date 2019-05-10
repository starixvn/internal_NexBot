namespace NexBot.Criteria.RegisterMessage
{
    public class RemoveIgnoredEventMessageCriteria : CommandCriteriaBase
    {
        public int CategoryId { get; set; }

        public string ConversationId { get; set; }

        public override string GetSettingKey() => "ConversationIgnored_Remove";

        public override bool IsValid() => CategoryId > 0 && !string.IsNullOrWhiteSpace(ConversationId);
    }
}