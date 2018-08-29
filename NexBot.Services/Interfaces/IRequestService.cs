namespace NexBot.Services.Interfaces
{
    using System.Threading.Tasks;

    public interface IRequestService
    {
        Task ApproveRequest(string conversationId, string commandRequest);
    }
}
