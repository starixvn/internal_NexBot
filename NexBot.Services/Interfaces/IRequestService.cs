namespace NexBot.Services.Interfaces
{
    using System.Threading.Tasks;
    using NexBot.Models.Request;

    public interface IRequestService
    {
        Task<string> ProcessRequest(string commandRequest, string command);

        Task<string> ProcessRequest(ProcessRequest processRequest);

        Task<string> ProcessMultipleRequests(ProcessRequest processRequest);
    }
}