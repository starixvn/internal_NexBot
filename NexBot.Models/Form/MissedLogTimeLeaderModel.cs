namespace NexBot.Models.Form
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Text.Json.Serialization;

    public class MissedLogTimeLeaderModel
    {
        public string ConversationId { get; set; }
        public string LeaderName { get; set; }
        public string StringMembers { get; set; }
        public IEnumerable<MemberModel> Members { get; set; }
    }


    public class MemberModel
    {
        public string NickName { get; set; }

        public int MissedCount { get; set; }
    }
}