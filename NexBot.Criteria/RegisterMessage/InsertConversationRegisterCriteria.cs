namespace NexBot.Criteria.RegisterMessage
{
    public class InsertConversationRegisterCriteria : CommandCriteriaBase
    {
        public int CategoryId { get; set; }

        public string ConversationId { get; set; }

        public override string GetSettingKey() => "ConversationRegister_Insert";

        public override bool IsValid() => CategoryId > 0 && !string.IsNullOrWhiteSpace(ConversationId);
    }
}