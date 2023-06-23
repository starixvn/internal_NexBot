namespace NexBot.Services.Interfaces
{
    using System.Threading.Tasks;
    using NexBot.Core.Models;
    using NexBot.Models;

    public interface IHRMConnector
    {
        Task<CommandResult> Send(ProcessRequest processRequest);
    }
}