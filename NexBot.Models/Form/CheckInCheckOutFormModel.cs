namespace NexBot.Models.Form
{
    using System;

    public class CheckInCheckOutFormModel
    {
        public int EmpId { get; set; }
        public byte MissingType { get; set; }
        public string Shift { get; set; }
        public DateTime? AttendanceDate { get; set; }
        public static string MaxDate => DateTime.Now.ToString("yyyy-MM-dd");

        public string RequestDateString
        {
            get
            {
                if (AttendanceDate.HasValue) return AttendanceDate.Value.ToString("yyyy-MM-dd");
                return string.Empty;
            }
        }

        public string RequestTimeString
        {
            get
            {
                if (AttendanceDate.HasValue) return AttendanceDate.Value.ToString("HH:mm");
                return string.Empty;
            }
        }
    }
}