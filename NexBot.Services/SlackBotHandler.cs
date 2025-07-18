using NexBot.Models.Slack;

namespace NexBot.Services
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using NexBot.Core;
    using NexBot.Core.Services.Interfaces;
    using NexBot.Models;
    using NexBot.Models.Request;
    using NexBot.Models.SubmittedForm;
    using NexBot.Services.Helpers;
    using NexBot.Services.Interfaces;

    public class SlackBotHandler(
         IRequestService requestService,
         IRegisterMessageService registerMessageService,
         IHRMConnector hrmConnector,
         IFormAdaptiveCardService formAdaptiveCardService,
         IApplicationSettingService applicationSetting,
         IMessageService messageService,
         IRegistrationMealService registrationMealService,
         ILogProviderService logProviderService,
         IHttpClientFactory httpClientFactory) : ISlackBotHandler
    {
        private const string baseUrl = "https://slack.com/api/views.publish";

        public async Task HandleInteractiveRequest(SlackInteractionPayload payload)
        {
            if (payload.Message == null || payload.Channel == null)
            {
                return;
            }
            foreach (var action in payload.Actions)
            {
                var botId = $"{payload.Message.BotId}:{payload.Team.Id}";
                var conversationId = $"{botId}:{payload.Channel.Id}";

                if (!int.TryParse(action.ActionId, out var actionId))
                {
                    return;
                }
                else if (actionId is (int)SubmitAction.ApproveRequest or (int)SubmitAction.ProcessRequest)
                {
                    await HandleSubmitProcessRequest(action.Value, conversationId);
                }
                else if (actionId == (int)SubmitAction.GetCheckInOutForm)
                {
                    await HandleGetCheckInOutForm(botId, action.Value, conversationId);
                }
                else if (actionId == (int)SubmitAction.SubmitForm)
                {
                    await HandleSubmitForm(payload.State.Values, action.Value, botId, conversationId);
                }
                else if (actionId == (int)SubmitAction.RegistrationMeal)
                {
                    await HandleSubmitRegistVegetarian(action.Value, conversationId);
                }
                else if (actionId == (int)SubmitAction.RegistryAll)
                {
                    await HandleRegistryAllCategoryMessage(action.Value, conversationId);
                }
                else if (actionId is >= (int)SubmitAction.RegistryNewFingerMachineAlert and <= (int)SubmitAction.RegistryViolateLateInEarlyOutDiscipline)
                {
                    await registerMessageService.RegisterEventMessage(conversationId, int.Parse(action.Value));
                    await messageService.SendAsync(conversationId, $"Registered successfully with category id: {action.Value}!");
                }
                else if (actionId == (int)SubmitAction.RemindCancelMeal)
                {
                    await HandleRemindCancelMeal(action.Value, conversationId);
                }
                else
                {
                    return;
                }
            }
        }

        public async Task PublishAppHomeView(SlackAppHomeViewPayload payload)
        {
            var httpClient = httpClientFactory.CreateClient();
            var path = Path.Combine(".", "Resources", "SlackBlockKit", "AppHomeCard.json");
            var block = SlackBlockKitHelper.GenerateSlackBlockKit(path, payload);
            using var request = new HttpRequestMessage(HttpMethod.Post, new Uri(baseUrl));
            request.Content = new StringContent(JsonConvert.SerializeObject(block), Encoding.UTF8, "application/json");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", payload.ApiToken);
            using var response = await httpClient.SendAsync(request);
        }

        private async Task HandleGetCheckInOutForm(string botId, string callbackUrl, string conversationId)
        {
            var block = await formAdaptiveCardService.GetCheckInCheckOutBlockKitForm(botId, callbackUrl, conversationId);
            if (block != null)
            {
                await messageService.SendBlockKitAsync(conversationId, block);
            }
            else
            {
                await messageService.SendAsync(conversationId, "Something wrong! Please try again or check with admin");
            }
        }

        private async Task HandleSubmitProcessRequest(string strData, string conversationId)
        {
            var processRequest = JsonConvert.DeserializeObject<ProcessRequest>(strData);
            var result = await requestService.ProcessRequest(processRequest);
            await messageService.SendAsync(conversationId, result);
        }

        private async Task HandleSubmitRegistVegetarian(string strData, string conversationId)
        {
            var registVegetarianMealMessage = JsonConvert.DeserializeObject<RegistVegetarianMealMessage>(strData);
            var result = await registrationMealService.RegistVegetarian(registVegetarianMealMessage);
            logProviderService.LogFileMessage($"Regist meal result: {JsonConvert.SerializeObject(result)}");
            await messageService.SendAsync(conversationId, result);

        }

        private async Task HandleSubmitForm(Dictionary<string, SlackActionElement> values, string strData, string botId, string conversationId)
        {
            var requestType = values["MissingType"].Data.SelectedOption.Value;
            var requestDate = values["RequestDate"].Data.SelectedDate;
            var requestTime = values["TimeSelection"].Data.SelectedTime;
            var timeParts = requestTime.Split(':');
            var data = new CheckInCheckOutFormData();
            try
            {
                data = JsonConvert.DeserializeObject<CheckInCheckOutFormData>(strData);
                data.MissingType = int.Parse(requestType);
                data.Hour = timeParts[0];
                data.Minute = timeParts[1];
                data.WorkDate = DateTime.ParseExact($"{requestDate} {requestTime}", "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
            }
            catch (Exception e)
            {

            }

            var commandResult = await hrmConnector.SubmitCheckInCheckOutRecord(data);
            if (string.IsNullOrEmpty(commandResult.Message))
            {
                var type = data.MissingType == 1 ? "Check In" : "Check Out";
                commandResult.Message = $"Create {type} request successfully!";
            }
            await messageService.SendAsync(conversationId, commandResult.Message);
        }

        private async Task HandleRegistryAllCategoryMessage(string value, string conversationId)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                await messageService.SendAsync(conversationId, "No categories provided.");
                return;
            }

            var tasks = new List<Task>();

            var categoryIds = value
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(id => id.Trim())
                .Where(id => int.TryParse(id, out _))
                .Select(int.Parse)
                .ToList();

            if (!categoryIds.Any())
            {
                await messageService.SendAsync(conversationId, "No valid category IDs found.");
                return;
            }

            foreach (var categoryId in categoryIds)
            {
                tasks.Add(registerMessageService.RegisterEventMessage(conversationId, categoryId));
            }

            await Task.WhenAll(tasks);
            await messageService.SendAsync(conversationId, "All selected categories registered successfully!");
        }
    
        private async Task HandleRemindCancelMeal(string strData, string conversationId)
        {
            var cancelMealFormData = JsonConvert.DeserializeObject<CancelMealFormData>(strData);
            var result = await registrationMealService.CancelMeal(cancelMealFormData);
            logProviderService.LogFileMessage($"Cancel meal result: {JsonConvert.SerializeObject(result)}");
            await messageService.SendAsync(conversationId, result);
        }
    }
}