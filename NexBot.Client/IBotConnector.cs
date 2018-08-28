using Microsoft.Bot.Connector;

namespace NexBot.Client
{
    public interface IBotConnector
    {
        string Send(string message, string conversationId);
        string Send1(string message, string conversationId, Attachment attachment);
    }
}