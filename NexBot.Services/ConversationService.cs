namespace NexBot.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Bot.Connector;
    using NexBot.Core.Models;
    using NexBot.Core.Services;
    using NexBot.Criteria.Conversation;
    using NexBot.Models;
    using NexBot.Services.Interfaces;
    using System.Linq;

    public class ConversationService : BaseService, IConversationService
    {
        public ConversationService()
        {
        }

        public async Task AddContact(Activity activity)
        {
            var conversation = GetContactByConversationId(activity.Conversation.Id);

            if (conversation == null)
            {
                await AddNew(activity);
            }

            await AddNew(activity);
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

        private async Task<CommandResult> AddNew(Activity activity)
        {
            var criteria = new InsertConversationCriteria
            {
                ConversationId = activity.Conversation.Id,
                ChannelId = activity.ChannelId,
                ServiceUrl = activity.ServiceUrl,
                UserName = activity.From.Name,
                IsGroup = false,
                IsActive = true,
                IsAdmin = false
            };

            return await ExecuteNonQueryAsync(criteria);
        }
    }
}
