namespace NexBot.Models.Form
{
    using System;
    using System.Collections.Generic;

    public class MissedLogTimeMemberModel
    {
        public string ConversationId { get; set; }
        public string NickName { get; set; }
        public int MissedCount { get; set; }
        public string LogDate { get; set; }
        public string LogMonth { get; set; }
    }

}