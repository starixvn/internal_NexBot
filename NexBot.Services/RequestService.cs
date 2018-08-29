namespace NexBot.Services
{
    using System;
    using System.Text;
    using System.Threading.Tasks;
    using NexBot.Core.Services;
    using NexBot.Services.Interfaces;
    using RestSharp;

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
                //var hrmConnector = new HRMConnector();
                //hrmConnector.Send(requestId, nickName);

                var webClient = new RestClient { Encoding = Encoding.UTF8 };

                webClient.BaseUrl = new Uri("http://10.18.200.114:8081/api/");

                var request = new RestRequest("/request/processrequest", Method.POST);
                request.AddHeader("content-type", "application/x-www-form-urlencoded");
                request.AddParameter("requestId", requestId.ToString());
                request.AddParameter("command", "approve");
                request.AddParameter("nickName", nickName);

                await messageService.AddSystemLog(request.ToString());

                var result = webClient.Execute(request);

                await messageService.AddSystemLog(result.StatusCode.ToString());

                await messageService.AddSystemLog("done");
            }

        }
    }
}
