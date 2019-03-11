namespace NexBot.Criteria.Conversation
{
    using Fanex.Data.Repository;

    public class GetAllConversationUserCriteria : CriteriaBase
    {
        public override string GetSettingKey() => "ConversationUser_GetAll";

        public override bool IsValid() => true;
    }
}
