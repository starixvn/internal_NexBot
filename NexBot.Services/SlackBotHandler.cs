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
    using NexBot.Models.SubmittedForm;
    using NexBot.Services.Helpers;
    using NexBot.Services.Interfaces;

    public class SlackBotHandler(
         IHRMConnector hrmConnector,
         IFormAdaptiveCardService formAdaptiveCardService,
         IApplicationSettingService applicationSetting,
         IMessageService messageService,
         IHttpClientFactory httpClientFactory) : ISlackBotHandler
    {

        private const string baseUrl = "https://slack.com/api/views.publish";

    
        public async Task HandleInteractiveRequest(SlackInteractionPayload payload)
        {
            foreach (var action in payload.Actions)
            {
                var botId = $"{payload.Message.BotId}:{payload.Team.Id}";
                var conversationId = $"{botId}:{payload.Channel.Id}";
                if (action.ActionId == ((int)SubmitAction.GetCheckInOutForm).ToString())
                {
                    var block = await formAdaptiveCardService.GetCheckInCheckOutBlockKitForm(botId, conversationId);
                    if (block != null)
                    {
                        await messageService.SendBlockKitAsync(conversationId, block);
                    }
                    else
                    {
                        await messageService.SendAsync(conversationId, "Something wrong! Please try again or check with admin");

                    }
                }
                else if (action.ActionId == ((int)SubmitAction.SubmitForm).ToString()) {
                    var data = JsonConvert.DeserializeObject<CheckInCheckOutFormData>(action.Value);
                    await HandleSubmitForm(payload.State.Values, data, botId, conversationId);
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

        private async Task HandleSubmitForm(Dictionary<string, SlackActionElement> values, CheckInCheckOutFormData data,string botId, string conversationId)
        {
            var requestType = values["MissingType"].Data.SelectedOption.Value;
            var requestDate = values["RequestDate"].Data.SelectedDate;
            var requestTime = values["TimeSelection"].Data.SelectedTime;
            var timeParts = requestTime.Split(':');

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