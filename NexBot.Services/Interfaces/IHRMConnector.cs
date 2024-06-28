namespace NexBot.Services.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using NexBot.Core.Models;
    using NexBot.Models.Request;

    public interface IHRMConnector
    {
        Task<CommandResult> Send(ProcessRequest processRequest);

        Task<IEnumerable<ProcessRequestResult>> SendMultipleRequest(ProcessRequest processRequest);
    }
}