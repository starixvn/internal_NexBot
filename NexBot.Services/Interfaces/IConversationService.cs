namespace NexBot.Services.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Bot.Schema;
    using NexBot.Core.Models;
    using NexBot.Models;

    public interface IConversationService
    {
        Task AddConversation(IActivity activity);

        Task<ConversationModel> GetConversationById(string conversationId);

        Task<ConversationModel> GetConversationBotById(string conversationId);

        Task<IList<ConversationModel>> GetAdminConversation();

        Task<bool> CheckIsBot(string conversationId);

        Task AddConversationGroup(IConversationUpdateActivity activity);

        Task AddMembersToGroup(string groupConversationId, string userConversationIds);

        Task<CommandResult> RemoveConversation(IConversationUpdateActivity activity);

        Task RemoveMembersOutGroup(string groupConversationId, string userConversationIds);
    }
}