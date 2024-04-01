namespace NexBot.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using NexBot.Core.Services.Interfaces;
    using NexBot.Criteria.Conversation;
    using NexBot.Models;
    using NexBot.Services.Interfaces;

    public class AccountService : IAccountService
    {
        private readonly IBaseService baseService;

        public AccountService(IBaseService baseService)
        {
            this.baseService = baseService;
        }

        public async Task<IList<ConversationUser>> GetAllConversationUser()
        {
            var criteria = new GetAllConversationUserCriteria();

            var conversationUsers = await baseService.GetDataAsync<ConversationUser>(criteria);

            return conversationUsers.ToList();
        }

        public async Task<IList<ConversationUser>> GetIgnoredAccountByMessageCategory(int messageCategoryId)
        {
            var criteria = new GetIgnoredAccountByMessageCategoryCriteria
            {
                MessageCategoryId = messageCategoryId
            };

            var conversationUsers = await baseService.GetDataAsync<ConversationUser>(criteria);

            return conversationUsers.ToList();
        }

        public async Task<IList<ConversationUser>> GetRegisteredConversationByMessageCategory(int messageCategoryId)
        {
            var criteria = new GetRegisteredConversationCriteria
            {
                MessageCategoryId = messageCategoryId
            };

            var conversationUsers = await baseService.GetDataAsync<ConversationUser>(criteria);

            return conversationUsers.ToList();
        }
    }
}