namespace NexBot.Services.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Bot.Connector;
    using NexBot.Core.Models;
    using NexBot.Models;

    public interface IConversationService
    {
        Task AddContact(Activity activity);

        Task<Conversation> GetContactByConversationId(string conversationId);

        Task AddNewMemberToGroup(string groupConversationId, string userConversationId);
        
        Task<IList<Conversation>> GetAdminConversation();

        Task<CommandResult> RemoveContact(Activity activity);
         
        Task RemoveMemberFromGroup(string groupConversationId, string userConversationId);
    }
}