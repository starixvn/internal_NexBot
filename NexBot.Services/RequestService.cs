namespace NexBot.Services
{
    using System.Threading.Tasks;
    using NexBot.Core.Services;
    using NexBot.Services.Interfaces;

    public class RequestService : BaseService, IRequestService
    {
        private readonly IConversationService conversationService;
        private readonly IHRMConnector hrmConnector;

        public RequestService(IConversationService conversationService,
            IHRMConnector hrmConnector)
        {            this.conversationService = conversationService;
            this.hrmConnector = hrmConnector;
        }

        public async Task ApproveRequest(string conversationId, string commandRequest)
        {
            var conversation = await conversationService.GetConversationById(conversationId);
            var nickName = conversation.NickName;
            var slitCommandRequest = commandRequest.Split(':');
            if (slitCommandRequest.Length == 2)
            {
                long requestId;
                long.TryParse(slitCommandRequest[1], out requestId);
                hrmConnector.Send(requestId, nickName);
            }
        }
    }
}