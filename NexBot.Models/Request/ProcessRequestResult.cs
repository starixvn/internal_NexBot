namespace NexBot.Models.Request
{
    public class ProcessRequestResult
    {
        public long RequestId { get; set; }
        public bool IsSuccessful { get; set; }
        public string Message { get; set; }
    }
}