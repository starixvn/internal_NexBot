namespace NexBot.Models.Form
{
    using System;

    public class CheckInCheckOutFormModel
    {
        public string ConversationId { get; set; }
        public byte RequestType { get; set; }
        public DateTime? RequestDate { get; set; }
        public static string MaxDate => DateTime.Now.ToString("yyyy-MM-dd");
        public string RequestDateString { get; set; }
        public string RequestTimeString { get; set; }
        public string Shift { get; set; }
    }
}