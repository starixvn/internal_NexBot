namespace NexBot.Models.Request
{
    using System;

    public class RequestMessage
    {
        public string ConversationId { get; set; }

        public long RequestId { get; set; }

        public int ProcessedBy { get; set; }

        public string Message { get; set; }

        public string CallBackUrl { get; set; }

        public string CreatedDate { get; set; }

        public string Supervisor { get; set; }

        public string Code { get; set; }

        public string Nick { get; set; }

        public string FullName { get; set; }

        public string RequestDate { get; set; }

        public string Shift { get; set; }

        public string Time { get; set; }

        public string LeaveType { get; set; }

        public string Remark { get; set; }

        public string RequestType { get; set; }

        public string AvatarUrl { get; set; }

        public bool IsReadOnly { get; set; }
    }
}