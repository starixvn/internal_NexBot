namespace NexBot.Services.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using NexBot.Core.Models;
    using NexBot.Models.CategoryMessage;

    public interface IRegisterMessageService
    {
        Task<string> GetCategoryMessages(string conversationId);

        Task<CommandResult> RegisterEventMessage(string conversationId, int categoryId);

        Task<CommandResult> RemoveRegisteredMessage(string conversationId, int categoryId);

        Task<IList<CategoryMessage>> GetAllCategoryMessage(string conversationId);

        Task<CommandResult> IgnoreEventMessage(string conversationId, int categoryId);

        Task<CommandResult> RemoveIgnoredEventMessage(string conversationId, int categoryId);
    }
}