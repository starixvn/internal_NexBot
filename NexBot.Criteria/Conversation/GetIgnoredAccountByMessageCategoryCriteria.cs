namespace NexBot.Criteria.Conversation
{
    using Fanex.Data.Repository;

    public class GetIgnoredAccountByMessageCategoryCriteria : CriteriaBase
    {
        public int MessageCategoryId { get; set; }

        public override string GetSettingKey() => "Conversation_GetIgnoredAccountByMessageCategory";

        public override bool IsValid() => MessageCategoryId > 0;
    }
}
