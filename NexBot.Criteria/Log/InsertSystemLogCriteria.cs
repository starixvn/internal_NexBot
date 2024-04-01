namespace NexBot.Criteria
{
    using NexBot.Core.Models;

    public class InsertSystemLogCommand : BaseCommand
    {
        public string LogContent { get; set; }

        public override string GetSettingKey() => "SystemLog_Insert";

        public override bool IsValid() => !string.IsNullOrWhiteSpace(LogContent);
    }
}
