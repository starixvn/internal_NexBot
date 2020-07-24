namespace NexBot.Criteria.Conversation
{
    using Fanex.Data.Repository;

    public class GetRegisteredConversationCriteria : CriteriaBase
    {
        public int MessageCategoryId { get; set; }

        public override string GetSettingKey() => "Conversation_GetRegisteredConversationByMessageCategory";

        public override bool IsValid() => MessageCategoryId > 0;
    }
}
