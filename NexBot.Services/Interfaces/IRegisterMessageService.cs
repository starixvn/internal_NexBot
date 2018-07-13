namespace NexBot.Services.Interfaces
{
    using System.Threading.Tasks;
    using NexBot.Core.Models;

    public interface IRegisterMessageService
    {
        Task<string> GetCategoryMessages(string conversationId);

        Task<CommandResult> RegisterEventMessage(string conversationId, int categoryId);

        Task<CommandResult> RemoveRegisteredMessage(string conversationId, int categoryId);
    }
}