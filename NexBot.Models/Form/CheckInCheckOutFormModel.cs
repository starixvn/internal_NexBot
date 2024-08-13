namespace NexBot.Models.Form
{
    using System;
    using System.Collections.Generic;

    public class CheckInCheckOutFormModel
    {
        public int EmpId { get; set; }
        public string NickName { get; set; }
        public byte MissingType { get; set; }
        public string Shift { get; set; }
        public DateTime? AttendanceDate { get; set; }
        public string MaxDate => DateTime.Now.ToString("yyyy-MM-dd");
        public string ConversationId { get; set; }

        public List<DropdownArray> HourArray
        {
            get
            {
                return InitDropdownArray(24);
            }
        }

        public List<DropdownArray> MinuteArray
        {
            get
            {
                return InitDropdownArray(60);
            }
        }

        public string DisplayDateString => AttendanceDate.Value.ToString("dd MMM, yyyy");

        public string RequestDateString
        {
            get
            {
                if (AttendanceDate.HasValue) return AttendanceDate.Value.ToString("yyyy-MM-dd");
                return string.Empty;
            }
        }

        public string Hour
        {
            get
            {
                if (AttendanceDate.HasValue) return AttendanceDate.Value.ToString("HH");
                return string.Empty;
            }
        }

        public string Minute
        {
            get
            {
                if (AttendanceDate.HasValue) return AttendanceDate.Value.ToString("mm");
                return string.Empty;
            }
        }

        private static List<DropdownArray> InitDropdownArray(int maxValue)
        {
            var values = new List<DropdownArray>();
            for (int i = 0; i < maxValue; i++)
            {
                values.Add(new DropdownArray
                {
                    Display = (i < 10 ? "0" : string.Empty) + i,
                    Value = (i < 10 ? "0" : string.Empty) + i
                });
            }
            return values;
        }
    }

    public class DropdownArray
    {
        public string Display { get; set; }
        public string Value { get; set; }
    }
}