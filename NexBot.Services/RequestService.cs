namespace NexBot.Services
{
    using System.Threading.Tasks;
    using NexBot.Core.Services;
    using NexBot.Models;
    using NexBot.Services.Interfaces;

    public class RequestService : BaseService, IRequestService
    {
        private readonly IConversationService conversationService;
        private readonly IHRMConnector hrmConnector;

        public RequestService(IConversationService conversationService,
            IHRMConnector hrmConnector)
        {
            this.conversationService = conversationService;
            this.hrmConnector = hrmConnector;
        }

        public async Task ProcessRequest(string conversationId, string commandRequest, string command)
        {
            var conversation = await conversationService.GetConversationById(conversationId);
            var slitCommandRequest = commandRequest.Split('_');
            if (slitCommandRequest.Length == 3)
            {
                long.TryParse(slitCommandRequest[1], out long requestId);
                var processRequest = new ProcessRequest
                {
                    RequestId = requestId,
                    NickName = conversation.NickName,
                    Command = command,
                    CallBackUrl = slitCommandRequest[2]
                };

                await hrmConnector.Send(processRequest);
            }
            else if (slitCommandRequest.Length == 4)
            {
                long.TryParse(slitCommandRequest[1], out long requestId);
                int.TryParse(slitCommandRequest[2], out int processedBy);
                var processRequest = new ProcessRequest
                {
                    RequestId = requestId,
                    ProcessedBy = processedBy,
                    Command = command,
                    CallBackUrl = slitCommandRequest[3]
                };

                await hrmConnector.Send(processRequest);
            }
        }
    }
}