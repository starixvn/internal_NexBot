namespace NexBot.Services.Interfaces
{
    using System.Threading.Tasks;
    using Microsoft.Bot.Schema;
    using NexBot.Models;

    public interface IMessageService
    {
        Task SendReplyAsync(IMessageActivity activity, string message);

        Task SendAsync(string conversationId, string messageText);

        Task SendRequestAsync(SendRequestMessage requestMessage);

        Task AddSystemLog(string logContent);

        Task ReSendErrorMeesage();
    }
}