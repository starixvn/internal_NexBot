using NexBot.Models.Slack;

namespace NexBot.Services
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
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
                if (string.IsNullOrEmpty(action.ActionId))
                {
                    return;
                }

                var botId = $"{payload.Message.BotId}:{payload.Team.Id}";
                var conversationId = $"{botId}:{payload.Channel.Id}";
                switch (int.Parse(action.ActionId))
                {
                    case (int)SubmitAction.ApproveRequest:
                    case (int)SubmitAction.ProcessRequest:
                        await HandleSubmitProcessRequest(action.Value, conversationId);
                        break;

                    case (int)SubmitAction.GetCheckInOutForm:
                        var block = await formAdaptiveCardService.GetCheckInCheckOutBlockKitForm(botId, conversationId);
                        if (block != null)
                        {
                            await messageService.SendBlockKitAsync(conversationId, block);
                        }
                        else
                        {
                            await messageService.SendAsync(conversationId, "Something wrong! Please try again or check with admin");
                        }
                        break;

                    case (int)SubmitAction.SubmitForm:
                        await HandleSubmitForm(payload.State.Values, action.Value, botId, conversationId);
                        break;

                    case (int)SubmitAction.RegistrationMeal:
                        await HandleSubmitRegistVegetarian(action.Value, conversationId);
                        break;

                    case (int)SubmitAction.RegistryNewFingerMachineAlert:
                    case (int)SubmitAction.RegistryBioChangeTimeAlert:
                        await registerMessageService.RegisterEventMessage(conversationId, int.Parse(action.Value));
                        await messageService.SendAsync(conversationId, "Registered successfully!");
                        break;

                    default:
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

        private async Task HandleSubmitForm(Dictionary<string, SlackActionElement> values, string strData,string botId, string conversationId)
        {
            var requestType = values["MissingType"].Data.SelectedOption.Value;
            var requestDate = values["RequestDate"].Data.SelectedDate;
            var requestTime = values["TimeSelection"].Data.SelectedTime;
            var timeParts = requestTime.Split(':');

            var data = JsonConvert.DeserializeObject<CheckInCheckOutFormData>(strData);
            data.MissingType = int.Parse(requestType);
            data.BaseUrl = applicationSetting.GetHrmUrl(botId);
            data.Hour = timeParts[0];
            data.Minute = timeParts[1];
            data.WorkDate = DateTime.ParseExact($"{requestDate} {requestTime}", "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
            
            var commandResult = await hrmConnector.SubmitCheckInCheckOutRecord(data);
            if (string.IsNullOrEmpty(commandResult.Message))
            {
                var type = data.MissingType == 1 ? "Check In" : "Check Out";
                commandResult.Message = $"Create {type} request successfully!";
            }
            await messageService.SendAsync(conversationId, commandResult.Message);
        }
    }
}