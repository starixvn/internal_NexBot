namespace NexBot.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using NexBot.Core.Services;
    using NexBot.Criteria.Conversation;
    using NexBot.Models;
    using NexBot.Services.Interfaces;

    public class AccountService : BaseService, IAccountService
    {
        public async Task<IList<ConversationUser>> GetAllConversationUser()
        {
            var criteria = new GetAllConversationUserCriteria();

            var conversationUsers = await GetDataAsync<ConversationUser>(criteria);

            return conversationUsers.ToList();
        }

        public async Task<IList<ConversationUser>> GetIgnoredAccountByMessageCategory(int messageCategoryId)
        {
            var criteria = new GetIgnoredAccountByMessageCategoryCriteria
            {
                MessageCategoryId = messageCategoryId
            };

            var conversationUsers = await GetDataAsync<ConversationUser>(criteria);

            return conversationUsers.ToList();
        }
    }
}