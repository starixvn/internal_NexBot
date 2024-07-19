namespace NexBot.Services.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using NexBot.Core.Models;
    using NexBot.Models.Form;
    using NexBot.Models.Request;

    public interface IHRMConnector
    {
        Task<CommandResult> SendRequest(ProcessRequest processRequest);

        Task<IEnumerable<ProcessRequestResult>> SendMultipleRequest(ProcessRequest processRequest);

        Task<CheckInCheckOutFormModel> GetCheckInCheckOutModel(string baseUrl, string conversationId);
    }
}