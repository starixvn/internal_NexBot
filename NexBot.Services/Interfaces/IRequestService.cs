namespace NexBot.Services.Interfaces
{
    using System.Threading.Tasks;

    public interface IRequestService
    {
        Task<string> ProcessRequest(string conversationId, string commandRequest, string command);
    }
}
