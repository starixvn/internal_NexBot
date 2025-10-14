namespace NexBot.Services.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using NexBot.Core.Models;
    using NexBot.Models.Form;
    using NexBot.Models.Request;
    using NexBot.Models.SubmittedForm;

    public interface IHRMConnector
    {
        Task<CommandResult> SendRequest(ProcessRequest processRequest);

        Task<IEnumerable<ProcessRequestResult>> SendMultipleRequest(ProcessRequest processRequest);

        Task<CheckInCheckOutFormModel> GetCheckInCheckOutModel(string baseUrl, string conversationId);

        Task<CommandResult> SubmitCheckInCheckOutRecord(CheckInCheckOutFormData request);

        Task<CommandResult> MappingConversation(MappingConversationRequest request);

        Task SendWebhookN8N(string conversationId, string userId, string message);
    }
}