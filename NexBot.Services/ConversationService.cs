namespace NexBot.Services
{
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
            var conversation = GetContactByConversationId(activity.Conversation.Id);

            if (conversation == null)
            {
                await AddNew(activity);
            }

            await AddNew(activity);
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

        private async Task<Conversation> GetContactByConversationId(string conversationId)
        {
            var criteria = new GetConversationByIdCriteria
            {
                ConversationId = conversationId
            };

            var conversation = await GetDataEntityAsync<Conversation>(criteria);

            return conversation;
        }

    }
}
