namespace NexBot.Services.Interfaces
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Bot.Connector;
    using NexBot.Models;

    public interface IMessageService
    {
        Task SendReplyAsync(IMessageActivity activity, string message);

        Task SendAsync(string conversationId, string messageText);

        Task SendRequestAsync(SendRequestMessage requestMessage);

        ConnectorClient CreateConnectorClient(Uri serviceUrl);

        Task AddSystemLog(string logContent);

        Task ReSendErrorMeesage();
    }
}