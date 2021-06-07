namespace NexBot.Services
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;
    using NexBot.Models;
    using NexBot.Services.Interfaces;

    public class HrmConnector : IHRMConnector
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IConfiguration configuration;

        public HrmConnector(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            this.configuration = configuration;
            this.httpClientFactory = httpClientFactory;
        }

        public async Task<string> Send(ProcessRequest processRequest)
        {
            string result = string.Empty;
            var baseUrl = processRequest.CallBackUrl;
            var token = await GetTokenAsync(baseUrl);

            var httpClient = httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var url = new Uri($"{baseUrl}/Request/ProcessRequest?requestId={processRequest.RequestId}&command={processRequest.Command}&nickName={processRequest.NickName}");

            using (var request = new HttpRequestMessage(HttpMethod.Post, url))
            {
                using (var response = await httpClient.SendAsync(request))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        result = await response.Content.ReadAsStringAsync();
                    }
                }
            }

            return result;
        }

        private async Task<string> GetTokenAsync(string baseUrl)
        {
            var url = new Uri($"{baseUrl}/Token/GetToken?username={configuration.GetSection("NexHRId").Value}&password={configuration.GetSection("NexHRPassword").Value}");

            var httpClient = httpClientFactory.CreateClient();

            using (var request = new HttpRequestMessage(HttpMethod.Post, url))
            {
                using (var response = await httpClient.SendAsync(request))
                {
                    var responseData = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<string>(responseData);
                }
            }
        }
    }
}