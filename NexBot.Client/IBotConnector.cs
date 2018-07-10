namespace NexBot.Client
{
    public interface IBotConnector
    {
        string Send(string message, string conversationId);
    }
}