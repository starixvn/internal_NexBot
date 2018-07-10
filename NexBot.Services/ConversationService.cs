namespace NexBot.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Bot.Connector;
    using NexBot.Core.Models;
    using NexBot.Core.Services;
    using NexBot.Criteria.Conversation;
    using NexBot.Models;
    using NexBot.Services.Interfaces;

    public class ConversationService : BaseService, IConversationService
    {
        public ConversationService()
        {
        }

        public async Task AddContact(Activity activity)
        {
            var criteria = new InsertConversationCriteria
            {
                ConversationId = activity.Conversation.Id,
                ChannelId = activity.ChannelId,
                ServiceUrl = activity.ServiceUrl,
                UserName = activity.From.Name,
                IsGroup = activity.Conversation.IsGroup,
                IsActive = true,
                IsAdmin = false
            };

            await ExecuteNonQueryAsync(criteria);
        }

        public async Task<Conversation> GetContactByConversationId(string conversationId)
        {
            var criteria = new GetConversationByIdCriteria
            {
                ConversationId = conversationId
            };

            var conversation = await GetDataEntityAsync<Conversation>(criteria);

            return conversation;
        }

        public async Task<IList<Conversation>> GetAdminConversation()
        {
            var criteria = new GetAdminConversationCriteria();

            var conversations = await GetDataAsync<Conversation>(criteria);

            return conversations.ToList();
        }

        public async Task AddNewMemberToGroup(string groupConversationId, string userConversationId)
        {
            var criteria = new InsertGroupUserConversationCriteria
            {
                GroupConversationId = groupConversationId,
                UserConversationId = userConversationId
            };

            await ExecuteNonQueryAsync(criteria);
        }

        public async Task RemoveMemberFromGroup(string groupConversationId, string userConversationId)
        {
            var criteria = new RemoveGroupUserConversationCriteria
            {
                GroupConversationId = groupConversationId,
                UserConversationId = userConversationId
            };

            await ExecuteNonQueryAsync(criteria);
        }

        public async Task<CommandResult> RemoveContact(Activity activity)
        {
            var criteria = new RemoveConversationByIdCriteria
            {
                ConversationId = activity.Conversation.Id
            };
            return await ExecuteNonQueryAsync(criteria);
        }
    }
}