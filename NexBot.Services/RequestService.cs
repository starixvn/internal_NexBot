namespace NexBot.Services
{
    using System.Threading.Tasks;
    using NexBot.Core.Services;
    using NexBot.Services.Interfaces;

    public class RequestService : BaseService, IRequestService
    {
        private readonly IConversationService conversationService;
        private readonly IMessageService messageService;
        private readonly IHRMConnector hrmConnector;

        public RequestService(IConversationService conversationService,
            IMessageService messageService,
            IHRMConnector hrmConnector)
        {
            this.conversationService = conversationService;
            this.messageService = messageService;
            this.hrmConnector = hrmConnector;
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
                hrmConnector.Send(requestId, nickName);

                //var webClient = new RestClient { Encoding = Encoding.UTF8 };
                //webClient.BaseUrl = new Uri("http://10.18.200.114:8081/api/");
                //var requestUrl = string.Format("Request/ProcessRequest?requestId={0}&command={1}&nickName={2}", requestId, "approve", nickName);

                //var request = new RestRequest(requestUrl, Method.POST);
                //request.AddHeader("content-type", "application/x-www-form-urlencoded");

                //await messageService.AddSystemLog(request.ToString());

                //var result = webClient.Execute(request);

                //await messageService.AddSystemLog(result.StatusCode.ToString());

                //await messageService.AddSystemLog("done");
            }
        }
    }
}