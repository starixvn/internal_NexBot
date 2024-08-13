namespace NexBot.Services
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Schema;
    using Newtonsoft.Json;
    using NexBot.Core;
    using NexBot.Core.Enums;
    using NexBot.Core.Models;
    using NexBot.Core.Services.Interfaces;
    using NexBot.Core.Utilities;
    using NexBot.Models;
    using NexBot.Models.CategoryMessage;
    using NexBot.Models.Request;
    using NexBot.Models.SubmittedForm;
    using NexBot.Services.Interfaces;

    public class TeamsBotHandler(IRequestService requestService,
         IRegistrationMealService registrationMealService,
         IRegisterMessageService registerMessageService,
         ILogProviderService logProviderService,
         IHRMConnector hrmConnector,
         IFormAdaptiveCardService formAdaptiveCardService,
         IApplicationSettingService applicationSetting) : ITeamsBotHandler
    {
        private const string SuccessMessageResponse = "✅ **{0}** receive message category _{1}_ successfully! Type _help_ to get more information";
        private const string FailedMessageResponse = "❌ **{0}** receive message category _{1}_ failed! Please contact admin";

        public async Task HandleSubmitData(ITurnContext<IMessageActivity> turnContext)
        {
            if (turnContext.Activity.Value != null)
            {
                logProviderService.LogFileMessage(turnContext.Activity.Value.ToString());
                var submitData = JsonConvert.DeserializeObject<AdaptiveSubmit>(turnContext.Activity.Value.ToString());

                switch (submitData.Type)
                {
                    case (int)SubmitAction.ProcessRequest:
                        await HandleSubmitProcessRequest(turnContext, submitData);
                        break;

                    case (int)SubmitAction.RegistrationMeal:
                        await HandleSubmitRegistVegetarian(turnContext, submitData);
                        break;

                    case (int)SubmitAction.ProcessMultipleRequests:
                        await HandleSubmitProcessMultipleRequests(turnContext, submitData);
                        break;

                    case (int)SubmitAction.StartStopMessageCategory:
                        await HandleStartStopMessageCategory(turnContext, JsonConvert.SerializeObject(submitData.ResponseData));
                        break;

                    case (int)SubmitAction.CommandMenu:
                        await HandleCommand(turnContext, JsonConvert.SerializeObject(submitData.ResponseData));
                        break;

                    case (int)SubmitAction.SubmitForm:
                        await HandleSubmitForm(turnContext, submitData.FormId, turnContext.Activity.Value.ToString());
                        break;

                    case (int)SubmitAction.GetCheckInOutForm:
                        Attachment att = await formAdaptiveCardService.GetCheckInCheckOutAdaptiveCardForm(turnContext.Activity.Recipient.Id, turnContext.Activity.Conversation.Id);
                        IMessageActivity activity = att is not null ? MessageFactory.Attachment(att) : MessageFactory.Text("Something wrong! Please try again or check with admin");
                        await turnContext.SendActivityAsync(activity);
                        break;

                    default:
                        break;
                }
            }
        }

        private async Task HandleSubmitForm(ITurnContext<IMessageActivity> turnContext, int formId, string formJsonValue)
        {
            switch (formId)
            {
                case (int)FormInfo.FormCheckInCheckOutRequest:
                    var data = JsonConvert.DeserializeObject<CheckInCheckOutFormData>(formJsonValue);
                    data.WorkDate = DateTime.ParseExact($"{data.RequestDate} {data.Hour}:{data.Minute}", "yyyy-MM-dd HH:mm", System.Globalization.CultureInfo.InvariantCulture);
                    data.BaseUrl = applicationSetting.GetHrmUrl(turnContext.Activity.Recipient.Id);
                    var commandResult = await hrmConnector.SubmitCheckInCheckOutRecord(data);
                    if (commandResult.IsSuccessful)
                    {
                        var type = data.MissingType == 1 ? "Check In" : "Check Out";
                        commandResult.Message = $"Create {type} request successfully!";
                    }
                    await turnContext.SendActivityAsync(MessageFactory.Text(commandResult.Message));
                    break;

                default:
                    break;
            }
        }

        private async Task HandleCommand(ITurnContext<IMessageActivity> turnContext, string jsonData)
        {
            var data = JsonConvert.DeserializeObject<MessageCategorySubmitData>(jsonData);
            string command = data.Command;

            switch (command)
            {
                case InteractedCommand.getid:
                    string replyMsg = $"Your conversationId: {turnContext.Activity.Conversation.Id}";
                    await turnContext.SendActivityAsync(MessageFactory.Text(replyMsg));
                    break;

                default:
                    break;
            }
        }

        private async Task HandleSubmitProcessRequest(ITurnContext<IMessageActivity> turnContext, AdaptiveSubmit submitData)
        {
            var processRequest = JsonConvert.DeserializeObject<ProcessRequest>(submitData.ResponseData.ToString());
            var result = await requestService.ProcessRequest(processRequest);
            await turnContext.SendActivityAsync(MessageFactory.Text(result));
        }

        private async Task HandleSubmitProcessMultipleRequests(ITurnContext<IMessageActivity> turnContext, AdaptiveSubmit submitData)
        {
            var processRequest = JsonConvert.DeserializeObject<ProcessRequest>(submitData.ResponseData.ToString());
            var result = await requestService.ProcessMultipleRequests(processRequest);
            await turnContext.SendActivityAsync(MessageFactory.Text(result));
        }

        private async Task HandleSubmitRegistVegetarian(ITurnContext<IMessageActivity> turnContext, AdaptiveSubmit submitData)
        {
            var registVegetarianMealMessage = JsonConvert.DeserializeObject<RegistVegetarianMealMessage>(submitData.ResponseData.ToString());
            var result = await registrationMealService.RegistVegetarian(registVegetarianMealMessage);
            logProviderService.LogFileMessage($"Regist meal result: {JsonConvert.SerializeObject(result)}");
            await turnContext.SendActivityAsync(MessageFactory.Text(result));
        }

        private async Task HandleStartStopMessageCategory(ITurnContext<IMessageActivity> turnContext, string jsonData)
        {
            string conversationId = turnContext.Activity.Conversation.Id;
            var data = JsonConvert.DeserializeObject<MessageCategorySubmitData>(jsonData);
            CommandResult commandResult;
            if (data.Command.Equals("Stop", StringComparison.OrdinalIgnoreCase))
            {
                commandResult = !data.IsNeedRegister ?
                    await registerMessageService.IgnoreEventMessage(conversationId, data.CategoryId) :
                    await registerMessageService.RemoveRegisteredMessage(conversationId, data.CategoryId);
            }
            else
            {
                commandResult = !data.IsNeedRegister ?
                    await registerMessageService.RemoveIgnoredEventMessage(conversationId, data.CategoryId) :
                    await registerMessageService.RegisterEventMessage(conversationId, data.CategoryId);
            }

            var msg = GenerateResponseMessage(commandResult, data);
            var activity = MessageFactory.Text(msg, msg);
            await turnContext.SendActivityAsync(activity);
        }

        private static string GenerateResponseMessage(CommandResult commandResult, MessageCategorySubmitData submittedData)
        {
            string msg = commandResult.IsSuccessful ?
                string.Format(SuccessMessageResponse, submittedData.Command, ((MessageCategoryEnum)submittedData.CategoryId).Description()) :
                string.Format(FailedMessageResponse, submittedData.Command, ((MessageCategoryEnum)submittedData.CategoryId).Description());

            return msg;
        }
    }
}