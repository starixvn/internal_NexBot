namespace NexBot.Services
{
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;
    using NexBot.Models;
    using NexBot.Services.Interfaces;
    using RestSharp;
    using System;
    using System.Text;

    public class HRMConnector : IHRMConnector
    {
        private readonly IRestClient webClient;
        private readonly IConfiguration configuration;

        public HRMConnector(IConfiguration configuration, IRestClient webClient)
        {
            this.configuration = configuration;
            this.webClient = webClient ?? new RestClient { Encoding = Encoding.UTF8 };
        }

        public string Send(ProcessRequest processRequest)
        {
            var baseUrl = processRequest.CallBackUrl;
            webClient.BaseUrl = new Uri(baseUrl);
            var requestUrl = "Request/ProcessRequest?requestId={requestId}&command={command}&nickName={nickName}";
            var token = GetToken(baseUrl);

            var request = new RestRequest(requestUrl, Method.POST);
            request.AddHeader("content-type", "application/x-www-form-urlencoded");
            request.AddHeader("Authorization", $"Bearer {JsonConvert.DeserializeObject(token).ToString()}");
            request.AddParameter("requestId", processRequest.RequestId, ParameterType.UrlSegment);
            request.AddParameter("command", processRequest.Command, ParameterType.UrlSegment);
            request.AddParameter("nickName", processRequest.NickName, ParameterType.UrlSegment);

            var result = webClient.Execute(request);

            return result.Content;
        }

        private string GetToken(string baseUrl)
        {
            webClient.BaseUrl = new Uri(baseUrl);
            var route = $"{baseUrl}/api/Token/GetToken/?username={configuration.GetSection("NexHRId").Value}&password={configuration.GetSection("NexHRPassword").Value}";
            var request = new RestRequest(route, Method.POST);
            var tokenReponse = webClient.Execute(request);
            return tokenReponse.Content;
        }
    }
}