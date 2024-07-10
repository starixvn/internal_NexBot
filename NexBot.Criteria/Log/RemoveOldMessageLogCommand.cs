namespace NexBot.Criteria.Log
{
    using NexBot.Core.Models;

    public class RemoveOldMessageLogCommand : BaseCommand
    {
        public override string GetSettingKey() => "MessageLog_RemoveOldLog";

        public override bool IsValid() => true;
    }
}