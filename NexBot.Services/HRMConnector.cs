namespace NexBot.Services
{
    using NexBot.Models;
    using NexBot.Services.Interfaces;
    using RestSharp;
    using System;
    using System.Text;

    public class HRMConnector : IHRMConnector
    {
        private readonly IRestClient webClient;
        public HRMConnector(IRestClient webClient)
        {
            this.webClient = webClient ?? new RestClient { Encoding = Encoding.UTF8 };
        }

        public string Send(ProcessRequest processRequest)
        {
            webClient.BaseUrl = new Uri(processRequest.CallBackUrl);
            var requestUrl = "Request/ProcessRequest?requestId={requestId}&command={command}&nickName={nickName}";

            var request = new RestRequest(requestUrl, Method.POST);
            request.AddHeader("content-type", "application/x-www-form-urlencoded");
            request.AddParameter("requestId", processRequest.RequestId, ParameterType.UrlSegment);
            request.AddParameter("command", processRequest.Command, ParameterType.UrlSegment);
            request.AddParameter("nickName", processRequest.NickName, ParameterType.UrlSegment);

            var result = webClient.Execute(request);

            return result.Content;
        }
    }
}