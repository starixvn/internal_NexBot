namespace NexBot.Services.Interfaces
{
    public interface IHRMConnector
    {
        string Send(long requestId, string nickName);
    }
}
