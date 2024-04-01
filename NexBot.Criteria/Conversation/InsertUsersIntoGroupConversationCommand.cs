namespace NexBot.Criteria.Conversation
{
    using NexBot.Core.Models;

    public class InsertUsersIntoGroupConversationCommand : BaseCommand
    {
        public string GroupConversationId { get; set; }

        public string UserConversationIds { get; set; }

        public override string GetSettingKey() => "GroupUserConversation_InsertMulti";

        public override bool IsValid() => !string.IsNullOrWhiteSpace(GroupConversationId)
                                        && !string.IsNullOrWhiteSpace(UserConversationIds);
    }
}