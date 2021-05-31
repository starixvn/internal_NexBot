namespace NexBot.Services.Interfaces
{
    using NexBot.Models;

    public interface IHRMConnector
    {
        string Send(ProcessRequest processRequest);
    }
}