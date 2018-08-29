namespace NexBot.Services
{
    using System.Threading.Tasks;
    using NexBot.Core.Services;
    using NexBot.Services.Interfaces;

    public class RequestService : BaseService, IRequestService
    {
        private readonly IConversationService conversationService;
        private readonly IMessageService messageService;

        public RequestService(IConversationService conversationService, IMessageService messageService)
        {
            this.conversationService = conversationService;
            this.messageService = messageService;
        }

        public async Task ApproveRequest(string conversationId, string commandRequest)
        {
            var conversation = await conversationService.GetConversationById(conversationId);
            var nickName = conversation.NickName;
            await messageService.AddSystemLog(nickName);
            var slitCommandRequest = commandRequest.Split(':');
            if (slitCommandRequest.Length == 2)
            {
                await messageService.AddSystemLog("aa");
                long requestId;
                long.TryParse(slitCommandRequest[1], out requestId);
                await messageService.AddSystemLog(requestId.ToString());
                var hrmConnector = new HRMConnector();
                hrmConnector.Send(requestId, nickName);

                await messageService.AddSystemLog("done");
            }

        }
    }
}
