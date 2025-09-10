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
    using NexBot.Models.SubmittedForm;
    using NexBot.Services.Interfaces;

    public class HrmConnector(IHttpClientFactory httpClientFactory, IConfiguration configuration, IApplicationSettingService applicationSetting) : IHRMConnector
    {
        public async Task<CheckInCheckOutFormModel> GetCheckInCheckOutModel(string baseUrl, string conversationId)
        {
            string url = $"api/request/GetCheckInCheckOutForm?conversationId={conversationId}";
            try
            {
                var model = await SendAsync<CheckInCheckOutFormModel>(baseUrl, url, HttpMethod.Post);

                return model;
            }
            catch (Exception ex)
            {
                return default;
                throw;
            }
        }

        public async Task<CommandResult> SubmitCheckInCheckOutRecord(CheckInCheckOutFormData request)
        {
            CommandResult commandResult;

            var url = $"api/request/SubmitFormCheckInCheckOut?EmployeeId={request.EmpId}&CheckInOutType={request.MissingType}&WorkDate={request.WorkDate}&Hour={request.Hour}&Minute={request.Minute}&Shift={request.Shift}";

            try
            {
                commandResult = await SendAsync<CommandResult>(request.CallbackUrl, url, HttpMethod.Post);
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

        public async Task<CommandResult> MappingConversation(MappingConversationRequest request)
        {
            CommandResult commandResult;
            var baseUrl = applicationSetting.GetHrmUrl(request.BotId);
            var url = $"Employee/MappingConversation?conversationId={request.ConversationId}&username={request.Username}";

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

        public async Task SendWebhookN8N(string conversationId, string userId, string message)
        {
            var url = $"https://n8n.dearhoney.vn/webhook/4f40ebd8-ee82-4484-8cf2-db2246b8cb1e?conversationId={conversationId}&userId={userId}&message={message}";
            await SendHttpRequestAsync<CommandResult>(url, HttpMethod.Get);
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

        private async Task<T> SendHttpRequestAsync<T>(string url, HttpMethod method)
        {
            var httpClient = httpClientFactory.CreateClient();
            using var request = new HttpRequestMessage(method, url);
            using var response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var jsonResult = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(jsonResult);
        }
        #endregion Base Send Request
    }
}