namespace NexBot.Services.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Bot.Connector;
    using NexBot.Models;

    public interface IConversationService
    {
        Task AddContact(Activity activity);

        Task<Conversation> GetContactByConversationId(string conversationId);

        Task<IList<Conversation>> GetAdminConversation();
    }
}
