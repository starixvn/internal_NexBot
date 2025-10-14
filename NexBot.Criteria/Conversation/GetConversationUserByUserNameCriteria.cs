namespace NexBot.Criteria.Conversation
{
    using Fanex.Data.Repository;

    public class GetConversationUserByUserNameCriteria : CriteriaBase
    {
        public string UserName { get; set; }

        public override string GetSettingKey() => "ConversationUser_GetByUserName";

        public override bool IsValid() => !string.IsNullOrEmpty(UserName);
    }
}
