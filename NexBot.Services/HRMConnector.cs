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
    using NexBot.Models.Form;
    using NexBot.Models.Request;
    using NexBot.Services.Interfaces;

    public class HrmConnector(IHttpClientFactory httpClientFactory, IConfiguration configuration) : IHRMConnector
    {
        public async Task<CheckInCheckOutFormModel> GetCheckInCheckOutModel(string baseUrl, string conversationId)
        {
            string url = $"GetCheckCheckOutModel?conversationId={conversationId}";
            try
            {
                return await SendAsync<CheckInCheckOutFormModel>(baseUrl, conversationId, HttpMethod.Get);
            }
            catch (Exception ex)
            {
                return default;
                throw;
            }
        }

        public async Task<CommandResult> SendRequest(ProcessRequest processRequest)
        {
            CommandResult commandResult;
            var baseUrl = processRequest.CallBackUrl;

            var url = $"Request/ProcessRequest?requestId={processRequest.RequestId}&command={processRequest.Command}&processedBy={processRequest.ProcessedBy}";

            try
            {
                commandResult = await SendAsync<CommandResult>(baseUrl, url, HttpMethod.Post);
            }
            catch (Exception ex)
            {
                commandResult = new CommandResult
                {
                    Code = 1,
                    Message = ex.Message
                };
            }

            return commandResult;
        }

        public async Task<IEnumerable<ProcessRequestResult>> SendMultipleRequest(ProcessRequest processRequest)
        {
            var baseUrl = processRequest.CallBackUrl;
            var url = $"Request/ProcessMultipleRequest?requestIds={processRequest.RequestIds}&command={processRequest.Command}&processedBy={processRequest.ProcessedBy}";
            var processResults = new List<ProcessRequestResult>();
            try
            {
                processResults = await SendAsync<List<ProcessRequestResult>>(baseUrl, url, HttpMethod.Post);
            }
            catch (Exception ex)
            {
                var requestIds = processRequest.RequestIds.Split(",");
                processResults = requestIds.Select(id => new ProcessRequestResult
                {
                    RequestId = long.Parse(id),
                    IsSuccessful = false,
                    Message = ex.Message
                }).ToList();
                throw;
            }

            return processResults;
        }

        #region Base Send Request

        private async Task<T> SendAsync<T>(string baseUrl, string pathUrl, HttpMethod method)
        {
            var token = await GetTokenAsync(baseUrl);
            var httpClient = httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var url = new Uri($"{baseUrl}/{pathUrl}");
            using (var request = new HttpRequestMessage(method, url))
            {
                using (var response = await httpClient.SendAsync(request))
                {
                    response.EnsureSuccessStatusCode();
                    var jsonResult = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<T>(jsonResult);
                }
            }
        }

        private async Task<string> GetTokenAsync(string baseUrl)
        {
            string usr = configuration.GetSection("NexHRId").Value,
                   pwd = configuration.GetSection("NexHRPassword").Value;
            var url = new Uri($"{baseUrl}/Token/GetToken?username={usr}&password={pwd}");

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

        #endregion Base Send Request
    }
}