namespace NexBot.Criteria.RegisterMessage
{
    using Fanex.Data.Repository;

    public class GetRegisterCategoryMessageCriteria : CriteriaBase
    {
        public string ConversationId { get; set; }
        public bool IsGroup { get; set; }

        public override string GetSettingKey() => "MessageCategory_GetRegisteredByConversationId";

        public override bool IsValid() => !string.IsNullOrWhiteSpace(ConversationId);
    }
}