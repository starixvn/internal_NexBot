namespace NexBot.Services
{
    using System;
    using System.Threading.Tasks;
    using NexBot.Core;
    using NexBot.Core.Services;
    using NexBot.Core.Services.Interfaces;
    using NexBot.Models;
    using NexBot.Services.Interfaces;

    public class RequestService : BaseService, IRequestService
    {
        private const string successMessage = "Request #{0} has been {1} successfully!";
        private const string failMessage = "Request #{0} has been {1} unsuccessfully! Please contact Admin for detail";

        private readonly IConversationService conversationService;
        private readonly IHRMConnector hrmConnector;
        private readonly ILogProviderService logProviderService;

        public RequestService(IConversationService conversationService,
            IHRMConnector hrmConnector,
            ILogProviderService logProviderService)
        {
            this.conversationService = conversationService;
            this.hrmConnector = hrmConnector;
            this.logProviderService = logProviderService;
        }

        public async Task<string> ProcessRequest(string conversationId, string commandRequest, string command)
        {
            var conversation = await conversationService.GetConversationById(conversationId);
            var slitCommandRequest = commandRequest.Split('_');
            long.TryParse(slitCommandRequest[1], out long requestId);

            var result = string.Empty;
            try
            {
                var processRequest = new ProcessRequest();
                if (slitCommandRequest.Length == 3)
                {
                    processRequest = new ProcessRequest
                    {
                        RequestId = requestId,
                        NickName = conversation.NickName,
                        Command = command,
                        CallBackUrl = slitCommandRequest[2]
                    };
                }
                else if (slitCommandRequest.Length == 4)
                {
                    int.TryParse(slitCommandRequest[2], out int processedBy);
                    processRequest = new ProcessRequest
                    {
                        RequestId = requestId,
                        ProcessedBy = processedBy,
                        Command = command,
                        CallBackUrl = slitCommandRequest[3]
                    };
                }

                var commandResult = await hrmConnector.Send(processRequest);
                if (commandResult.IsSuccessful)
                {
                    result = string.Format(successMessage, requestId, GetCommandText(command));
                }
                else
                {
                    result = string.Format(failMessage, requestId, GetCommandText(command));
                }
            }
            catch (Exception exception)
            {
                logProviderService.WriteLogFileInfo(exception.ToString());
                result = string.Format(failMessage, requestId, GetCommandText(command));
            }

            return result;
        }

        private string GetCommandText(string command)
        {
            return command.Equals(Constants.ApproveRequestCommand, StringComparison.CurrentCultureIgnoreCase) ? "approved" : "rejected"; 
        }
    }
}