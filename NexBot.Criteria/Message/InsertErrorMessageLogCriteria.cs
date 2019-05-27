namespace NexBot.Criteria.Message
{
    using Fanex.Data.Repository;

    public class InsertErrorMessageLogCriteria : CriteriaBase
    {
        public string ConversationId { get; set; }

        public string FromConversationId { get; set; }

        public string FromName { get; set; }

        public string MessageContent { get; set; }

        public override string GetSettingKey() => "ErrorMessageLog_Insert";

        public override bool IsValid() => !string.IsNullOrWhiteSpace(ConversationId)
            && !string.IsNullOrWhiteSpace(MessageContent);
    }
}
