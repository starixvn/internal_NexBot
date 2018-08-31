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
            webClient.BaseUrl = new Uri("https://hrm.nexcel.vn/api/");
            var requestUrl = "Request/ProcessRequest?requestId={requestId}&command={command}&nickName={nickName}";

            var request = new RestRequest(requestUrl, Method.POST);
            request.AddHeader("content-type", "application/x-www-form-urlencoded");
            request.AddParameter("requestId", requestId, ParameterType.UrlSegment);
            request.AddParameter("command", "Approve", ParameterType.UrlSegment);
            request.AddParameter("nickName", nickName, ParameterType.UrlSegment);

            var result = webClient.Execute(request);

            return result.Content;
        }

    }
}
