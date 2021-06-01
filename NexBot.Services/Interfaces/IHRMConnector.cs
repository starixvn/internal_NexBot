namespace NexBot.Services.Interfaces
{
    using System.Threading.Tasks;
    using NexBot.Models;

    public interface IHRMConnector
    {
        Task<string> Send(ProcessRequest processRequest);
    }
}