namespace NexBot.Services
{
    using System.Threading.Tasks;
    using NexBot.Core.Services;
    using NexBot.Services.Interfaces;

    public class RequestService : BaseService, IRequestService
    {
        private readonly IConversationService conversationService;

        public RequestService(IConversationService conversationService)
        {
            this.conversationService = conversationService;
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
                var hrmConnector = new HRMConnector();
                hrmConnector.Send(requestId, nickName);
            }

        }
    }
}
