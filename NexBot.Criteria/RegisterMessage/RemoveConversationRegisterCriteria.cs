namespace NexBot.Criteria.RegisterMessage
{
    public class RemoveConversationRegisterCriteria : CommandCriteriaBase
    {
        public int CategoryId { get; set; }

        public string ConversationId { get; set; }

        public override string GetSettingKey() => "ConversationRegister_Remove";

        public override bool IsValid() => CategoryId > 0 && !string.IsNullOrWhiteSpace(ConversationId);
    }
}