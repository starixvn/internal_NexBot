namespace NexBot.Criteria.Message
{
    using NexBot.Core.Models;

    public class InsertHRMessageLogCommand : BaseCommand
    {
        public string FromName { get; set; }

        public string ToName { get; set; }

        public string MessageContent { get; set; }

        public bool IsFromHR { get; set; }

        public override string GetSettingKey() => "HRMessageLog_Insert";

        public override bool IsValid() => !string.IsNullOrWhiteSpace(MessageContent)
            && !string.IsNullOrWhiteSpace(FromName)
            && !string.IsNullOrWhiteSpace(ToName);
    }
}
