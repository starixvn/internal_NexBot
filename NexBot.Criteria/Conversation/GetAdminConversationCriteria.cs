namespace NexBot.Criteria.Conversation
{
    using Fanex.Data.Repository;

    public class GetAdminConversationCriteria : CriteriaBase
    {
        public override string GetSettingKey() => "Conversation_GetAdmin";

        public override bool IsValid() => true;
    }
}
