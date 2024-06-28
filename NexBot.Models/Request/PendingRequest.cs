namespace NexBot.Models.Request
{
    using System.Collections.Generic;

    public class PendingRequest
    {
        public int ApproverId { get; set; }
        public string ApproverNickname { get; set; }
        public string RequestIds { get; set; }
        public int TotalRequest { get; set; }
        public string ConversationId { get; set; }
        public string StringRequests { get; set; }
        public string CallbackUrl { get; set; }
        public IEnumerable<MemberRequest> Requests { get; set; }
    }

    public class MemberRequest
    {
        public long RequestId { get; set; }
        public string NickName { get; set; }
        public string FullName { get; set; }
        public string FromDate { get; set; }
        public string TeamName { get; set; }
        public string Avatar { get; set; }
        public string DateDetails { get; set; }
        public string RequestType { get; set; }
        public string TotalDays { get; set; }
        public string Remark { get; set; }
        public bool IsLeaveRequest { get; set; }
        public string CreatedDate { get; set; }
        public string CallbackUrl { get; set; }
        public int ApproverId { get; set; }
    }
}