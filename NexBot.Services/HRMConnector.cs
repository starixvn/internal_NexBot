namespace NexBot.Services
{
    using System;
    using System.Text;
    using NexBot.Services.Interfaces;
    using RestSharp;

    public class HRMConnector: IHRMConnector
    {
        private readonly IRestClient webClient;

        public HRMConnector() : this(null)
        {
        }

        protected internal HRMConnector(IRestClient webClient)
        {
            this.webClient = webClient ?? new RestClient { Encoding = Encoding.UTF8 };
        }

        public string Send(long requestId, string nickName)
        {
            var result = ForwardToHRM(requestId, nickName);

            return result;
        }

        private string ForwardToHRM(long requestId, string nickName)
        {
            webClient.BaseUrl = new Uri("http://10.18.200.114:8081/api");

            var request = new RestRequest("/Request/ProcessRequest", Method.POST);
            request.AddHeader("content-type", "application/x-www-form-urlencoded");
            request.AddParameter("requestId", requestId);
            request.AddParameter("command", "Approve");
            request.AddParameter("nickName", nickName);

            var result = webClient.Execute(request);

            return result.Content;
        }

    }
}
