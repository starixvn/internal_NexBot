namespace NexBot.Services.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using NexBot.Models;

    public interface IAccountService
    {
        Task<IList<ConversationUser>> GetAllConversationUser();
        
        Task<ConversationUser> GetConversationUserByUserName(string userName);

        Task<IList<ConversationUser>> GetIgnoredAccountByMessageCategory(int messageCategoryId);

        Task<IList<ConversationUser>> GetRegisteredConversationByMessageCategory(int messageCategoryId);
    }
}
