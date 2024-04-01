namespace NexBot.Criteria.Message
{
    using NexBot.Core.Models;

    public class InsertMessageLogCommand : BaseCommand
    {
        public string ConversationId { get; set; }

        public string FromConversationId { get; set; }

        public string FromName { get; set; }

        public string MessageContent { get; set; }

        public override string GetSettingKey() => "MessageLog_Insert";

        public override bool IsValid() => !string.IsNullOrWhiteSpace(ConversationId)
            && !string.IsNullOrWhiteSpace(MessageContent);
    }
}
