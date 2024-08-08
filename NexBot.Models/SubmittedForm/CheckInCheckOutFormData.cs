using System;

namespace NexBot.Models.SubmittedForm
{
    public class CheckInCheckOutFormData
    {
        public int EmpId { get; set; }
        public int MissingType { get; set; }
        public string RequestDate { get; set; }
        public string Hour { get; set; }
        public string Minute { get; set; }
        public DateTime WorkDate { get; set; }
        public string BaseUrl { get; set; }
    }
}