namespace NexBot.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Bot.Schema;
    using NexBot.Core.Models;
    using NexBot.Core.Services;
    using NexBot.Criteria.Conversation;
    using NexBot.Models;
    using NexBot.Services.Interfaces;

    public class ConversationService : BaseService, IConversationService
    {
        public async Task AddConversation(IConversationUpdateActivity activity)
        {
            var criteria = new InsertConversationCriteria
            {
                ConversationId = activity.Conversation.Id,
                ChannelId = activity.ChannelId,
                ServiceUrl = activity.ServiceUrl,
                UserName = activity.From.Name,
                IsGroup = activity.Conversation.IsGroup,
                IsActive = true,
                IsAdmin = false,
                ConversationBotId = activity.Recipient.Id,
                ConversationBotName = activity.Recipient.Name
            };

            await ExecuteNonQueryAsync(criteria);
        }

        public async Task<ConversationModel> GetConversationById(string conversationId)
        {
            var criteria = new GetConversationByIdCriteria
            {
                ConversationId = conversationId
            };

            var conversation = await GetDataEntityAsync<ConversationModel>(criteria);

            return conversation;
        }

        public async Task<ConversationModel> GetConversationBotById(string conversationId)
        {
            var criteria = new GetConversationBotByIdCriteria
            {
                ConversationId = conversationId
            };

            var conversation = await GetDataEntityAsync<ConversationModel>(criteria);

            return conversation;
        }

        public async Task<bool> CheckIsBot(string conversationId)
        {
            var conversation = await GetConversationBotById(conversationId);
            var isBot = conversation != null && !string.IsNullOrWhiteSpace(conversation.ConversationId);
            return isBot;
        }

        public async Task<IList<ConversationModel>> GetAdminConversation()
        {
            var criteria = new GetAdminConversationCriteria();

            var conversations = await GetDataAsync<ConversationModel>(criteria);

            return conversations.ToList();
        }

        public async Task AddConversationGroup(IConversationUpdateActivity activity)
        {
            var conversation = await GetConversationById(activity.Conversation.Id);

            if (conversation == null)
            {
                await AddConversation(activity);
            }
        }

        public async Task AddMembersToGroup(string groupConversationId, string userConversationIds)
        {
            var criteria = new InsertUsersIntoGroupConversationCriteria
            {
                GroupConversationId = groupConversationId,
                UserConversationIds = userConversationIds
            };

            await ExecuteNonQueryAsync(criteria);
        }

        public async Task RemoveMembersOutGroup(string groupConversationId, string userConversationIds)
        {
            var criteria = new RemoveUsersOutGroupConversationCriteria
            {
                GroupConversationId = groupConversationId,
                UserConversationIds = userConversationIds
            };

            await ExecuteNonQueryAsync(criteria);
        }

        public async Task<CommandResult> RemoveConversation(IConversationUpdateActivity activity)
        {
            var criteria = new RemoveConversationByIdCriteria
            {
                ConversationId = activity.Conversation.Id
            };
            return await ExecuteNonQueryAsync(criteria);
        }

        private async Task InsertMemberConversation(ChannelAccount account)
        {
            var criteria = new InsertConversationByChannelAccountCriteria
            {
                ConversationId = account.Id,
                UserName = account.Name
            };

            await ExecuteNonQueryAsync(criteria);
        }

        public async Task InsertGroupMembersInfo(IList<ChannelAccount> members)
        {
            foreach (var member in members)
            {
                await InsertMemberConversation(member);
            }
        }
    }
}