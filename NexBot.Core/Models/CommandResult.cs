namespace NexBot.Core.Models
{
    public class CommandResult
    {
        public CommandResult()
        {
            Code = 1;
            Message = Resources.Message.InvalidData;
        }

        public int Code { get; set; }

        public bool IsSuccessful => Code == 0;

        public string Message { get; set; }
    }
}