namespace NexBot.Criteria.RegisterMessage
{
    public class IgnoreEventMessageCriteria : CommandCriteriaBase
    {
        public int CategoryId { get; set; }

        public string ConversationId { get; set; }

        public override string GetSettingKey() => "ConversationIgnored_Insert";

        public override bool IsValid() => CategoryId > 0 && !string.IsNullOrWhiteSpace(ConversationId);
    }
}