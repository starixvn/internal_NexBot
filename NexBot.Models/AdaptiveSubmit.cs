namespace NexBot.Models
{
    public class AdaptiveSubmit
    {
        public int Type { get; set; }
        public int MessageCategory { get; set; }
        public int FormId { get; set; }
        public object ResponseData { get; set; }
    }
}