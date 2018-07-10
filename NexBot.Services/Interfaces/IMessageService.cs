namespace NexBot.Utilitites
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Bot.Connector;
    using NexBot.Models;

    public interface IMessageService
    {
        Task SendAsync(IMessageActivity activity, string message);

        Task SendAsync(string conversationId, string messageText, string serviceUrl);

        ConnectorClient CreateConnectorClient(Uri serviceUrl);
    }
}