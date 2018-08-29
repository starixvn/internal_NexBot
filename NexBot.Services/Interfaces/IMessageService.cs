namespace NexBot.Services.Interfaces
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Bot.Connector;

    public interface IMessageService
    {
        Task SendReplyAsync(IMessageActivity activity, string message);

        Task SendAsync(string conversationId, string messageText);

        Task SendRequestAsync(string conversationId, long requestId, string messageText);

        ConnectorClient CreateConnectorClient(Uri serviceUrl);

        Task AddSystemLog(string logContent);
    }
}