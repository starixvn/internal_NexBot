namespace NexBot.Services.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Bot.Schema;
    using NexBot.Core.Models;
    using NexBot.Criteria.Conversation;
    using NexBot.Models;

    public interface IConversationService
    {
        Task AddConversationAsync(IActivity activity);

        Task AddConversationAsync(InsertConversationCommand command);

        Task<CommandResult> RemoveConversationAsync(IActivity activity);

        Task<ConversationModel> GetConversationById(string conversationId);

        Task<ConversationModel> GetConversationBotById(string conversationId);

        Task<IList<ConversationModel>> GetAdminConversation();

        Task<IEnumerable<ApplicationSetting>> GetApplicationSettingAsync();

        Task<IEnumerable<Bot>> GetBotAsync();
    }
}