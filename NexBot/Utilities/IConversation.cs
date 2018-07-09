namespace NexBot.Utilitites
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Bot.Connector;
    using NexBot.Models;

    public interface IConversation
    {
        Task SendAsync(IMessageActivity activity, string message);

        Task SendAsync(MessageInfo messageInfo);

        Task<string> SendAsync(string conversationId, string message);

        Task SendAdminAsync(string message);

        ConnectorClient CreateConnectorClient(Uri serviceUrl);
    }
}