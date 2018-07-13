namespace NexBot.Criteria.RegisterMessage
{
    using Fanex.Data.Repository;

    public class GetCategoryMessageCriteria : CriteriaBase
    {
        public string ConversationId { get; set; }

        public bool IsRegistered { get; set; }

        public override string GetSettingKey() => "CategoryMessage_Get";

        public override bool IsValid() => !string.IsNullOrWhiteSpace(ConversationId);
    }
}