namespace NexBot.Criteria.RegisterMessage
{
    using NexBot.Core.Models;

    public class IgnoreEventMessageCommand : BaseCommand
    {
        public int CategoryId { get; set; }

        public string ConversationId { get; set; }

        public override string GetSettingKey() => "ConversationIgnored_Insert";

        public override bool IsValid() => CategoryId > 0 && !string.IsNullOrWhiteSpace(ConversationId);
    }
}