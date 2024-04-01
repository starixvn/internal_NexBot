namespace NexBot.Criteria.RegisterMessage
{
    using NexBot.Core.Models;

    public class RemoveIgnoredEventMessageCommand : BaseCommand
    {
        public int CategoryId { get; set; }

        public string ConversationId { get; set; }

        public override string GetSettingKey() => "ConversationIgnored_Remove";

        public override bool IsValid() => CategoryId > 0 && !string.IsNullOrWhiteSpace(ConversationId);
    }
}