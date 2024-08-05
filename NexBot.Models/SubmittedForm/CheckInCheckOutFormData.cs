using System;

namespace NexBot.Models.SubmittedForm
{
    public class CheckInCheckOutFormData
    {
        public int EmpId { get; set; }
        public int MissingType { get; set; }
        public string RequestDate { get; set; }
        public string RequestTime { get; set; }
        public DateTime Attendance { get; set; }
    }
}