namespace NexBot.Models
{
    public class TimesheetRecord
    {
        public string ConversationId { get; set; }
        public string NickName { get; set; }
        public string CheckoutTime { get; set; }
        public string CheckinTime { get; set; }
        public string TotalHours { get; set; }
        public string MessageContent { get; set; }
    }
}