namespace NexBot.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;
    using NexBot.Core.Models;
    using NexBot.Models.Request;
    using NexBot.Services.Interfaces;

    public class HrmConnector(IHttpClientFactory httpClientFactory, IConfiguration configuration) : IHRMConnector
    {
        public async Task<CommandResult> Send(ProcessRequest processRequest)
        {
            CommandResult commandResult;
            var baseUrl = processRequest.CallBackUrl;
            var token = await GetTokenAsync(baseUrl);

            var httpClient = httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var url = new Uri($"{baseUrl}/Request/ProcessRequest?requestId={processRequest.RequestId}&command={processRequest.Command}&processedBy={processRequest.ProcessedBy}");

            using (var request = new HttpRequestMessage(HttpMethod.Post, url))
            {
                using (var response = await httpClient.SendAsync(request))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadAsStringAsync();
                        commandResult = JsonConvert.DeserializeObject<CommandResult>(result);
                    }
                    else
                    {
                        commandResult = new CommandResult
                        {
                            Code = 1,
                            Message = response.ReasonPhrase
                        };
                    }
                }
            }

            return commandResult;
        }

        public async Task<IEnumerable<ProcessRequestResult>> SendMultipleRequest(ProcessRequest processRequest)
        {
            var baseUrl = processRequest.CallBackUrl;
            var token = await GetTokenAsync(baseUrl);
            var processResults = new List<ProcessRequestResult>();
            var httpClient = httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var url = new Uri($"{baseUrl}/Request/ProcessMultipleRequest?requestIds={processRequest.RequestIds}&command={processRequest.Command}&processedBy={processRequest.ProcessedBy}");

            using (var request = new HttpRequestMessage(HttpMethod.Post, url))
            {
                using (var response = await httpClient.SendAsync(request))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadAsStringAsync();
                        processResults = JsonConvert.DeserializeObject<List<ProcessRequestResult>>(result);
                    }
                    else
                    {
                        var requestIds = processRequest.RequestIds.Split(",");
                        processResults = requestIds.Select(id => new ProcessRequestResult
                        {
                            RequestId = long.Parse(id),
                            IsSuccessful = false,
                            Message = response.ReasonPhrase
                        }).ToList();
                    }
                }
            }
            return processResults;
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
                    return responseData;
                }
            }
        }
    }
}