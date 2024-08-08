namespace NexBot.Bots
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Teams;
    using Microsoft.Bot.Schema;
    using Newtonsoft.Json;
    using NexBot.Bots.Interfaces;
    using NexBot.Core;
    using NexBot.Core.Models;
    using NexBot.Core.Services.Interfaces;
    using NexBot.Core.Utilities;
    using NexBot.Criteria.Conversation;
    using NexBot.Models;
    using NexBot.Models.CategoryMessage;
    using NexBot.Models.Request;
    using NexBot.Models.SubmittedForm;
    using NexBot.Services.Interfaces;

    public class TeamsBot(IMessageService messageService,
         IRequestService requestService,
         IRegistrationMealService registrationMealService,
         IRegisterMessageService registerMessageService,
         ILogProviderService logProviderService,
         IConversationService conversationService,
         IFormAdaptiveCardService formAdaptiveCardService,
         IHRMConnector hrmConnector,
         IApplicationSettingService applicationSetting) : TeamsActivityHandler, ITeamsBot
    {
        private const string greetingMessage = "Hello, I am {0}";
        private const string defaultReplyMessage = "Not supported command, type **help** to view available supported commands. Thanks & have a nice day";
        private const string botNameIgnoreMsg = "Terra-Bot";

        protected override async Task OnConversationUpdateActivityAsync(ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            if (turnContext.Activity.MembersAdded != null)
            {
                if (turnContext.Activity.MembersAdded.Any(m => m.Id == turnContext.Activity.Recipient?.Id))
                {
                    await AddConversationAsync(turnContext, cancellationToken);
                }
            }
            else
            {
                if (turnContext.Activity.MembersRemoved != null
                && turnContext.Activity.MembersRemoved.Any(m => m.Id == turnContext.Activity.Recipient?.Id))
                {
                    await conversationService.RemoveConversationAsync(turnContext.Activity);
                }
            }
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var botName = turnContext.Activity.Recipient?.Name ?? string.Empty;
            var message = turnContext.Activity.Text?.Trim() ?? string.Empty;
            if (turnContext.Activity.Conversation.IsGroup == true)
            {
                message = BotHelper.GetMessageInGroupByChannel(message, turnContext.Activity.ChannelId, botName);
            }

            if (botName.Equals(botNameIgnoreMsg))
            {
                if (message.Equals(InteractedCommand.getid, StringComparison.OrdinalIgnoreCase))
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text(turnContext.Activity.Recipient.Id), cancellationToken);
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(turnContext.Activity.Text))
                {
                    CommandResult commandResult;
                    switch (message)
                    {
                        case InteractedCommand.help:
                            var attachment = await messageService.GetAttachmentHelpAdaptiveCard(
                                turnContext.Activity.Conversation.Id,
                                turnContext.Activity.Conversation.IsGroup ?? false,
                                botName);
                            var msg = MessageFactory.Attachment(attachment);
                            await turnContext.SendActivityAsync(msg, cancellationToken);
                            break;

                        case InteractedCommand.getid:
                            string replyMsg = $"Your conversationId: {turnContext.Activity.Conversation.Id}";
                            await turnContext.SendActivityAsync(MessageFactory.Text(replyMsg), cancellationToken);
                            break;

                        case InteractedCommand.getbot:
                            var botInfo = $"BotId: {turnContext.Activity.Recipient.Id}, BotName: {botName}";
                            await turnContext.SendActivityAsync(MessageFactory.Text(botInfo), cancellationToken);
                            break;

                        case InteractedCommand.regist:
                            await conversationService.AddConversationAsync(turnContext.Activity);
                            break;

                        case InteractedCommand.stop_remind_vegetarian_meal:
                            commandResult = await registerMessageService.IgnoreEventMessage(turnContext.Activity.Conversation.Id, (int)MessageCategoryEnum.ReminderVegetarianMeal);
                            if (commandResult.IsSuccessful)
                            {
                                await turnContext.SendActivityAsync(MessageFactory.Text(commandResult.Message, commandResult.Message), cancellationToken);
                            }
                            break;

                        case InteractedCommand.start_remind_vegetarian_meal:
                            commandResult = await registerMessageService.RemoveIgnoredEventMessage(turnContext.Activity.Conversation.Id, (int)MessageCategoryEnum.ReminderVegetarianMeal);
                            if (commandResult.IsSuccessful)
                            {
                                await turnContext.SendActivityAsync(MessageFactory.Text(commandResult.Message, commandResult.Message), cancellationToken);
                            }
                            break;

                        case InteractedCommand.stop_remind_reachedworkingtime:
                            commandResult = await registerMessageService.IgnoreEventMessage(turnContext.Activity.Conversation.Id, (int)MessageCategoryEnum.ReminderReachedWorkingHour);
                            if (commandResult.IsSuccessful)
                            {
                                await turnContext.SendActivityAsync(MessageFactory.Text(commandResult.Message, commandResult.Message), cancellationToken);
                            }
                            break;

                        case InteractedCommand.start_remind_reachedworkingtime:
                            commandResult = await registerMessageService.RemoveIgnoredEventMessage(turnContext.Activity.Conversation.Id, (int)MessageCategoryEnum.ReminderReachedWorkingHour);
                            if (commandResult.IsSuccessful)
                            {
                                await turnContext.SendActivityAsync(MessageFactory.Text(commandResult.Message, commandResult.Message), cancellationToken);
                            }
                            break;

                        case InteractedCommand.stop_notify_checkout:
                            await HandleStopReceiveNotificationByType(turnContext, MessageCategoryEnum.NotifyCheckOut, SubmitAction.StopCheckOutNotification);
                            break;

                        case InteractedCommand.start_notify_checkout:
                            commandResult = await registerMessageService.RemoveIgnoredEventMessage(turnContext.Activity.Conversation.Id, (int)MessageCategoryEnum.NotifyCheckOut);
                            if (commandResult.IsSuccessful)
                            {
                                var msgResponse = "Resgiter receive checkout notification successfully! ✅";
                                await turnContext.SendActivityAsync(MessageFactory.Text(msgResponse, msgResponse), cancellationToken);
                            }
                            break;

                        case InteractedCommand.stop_notify_checkin:
                            await HandleStopReceiveNotificationByType(turnContext, MessageCategoryEnum.NotifyCheckIn, SubmitAction.StopCheckInNotification);
                            break;

                        case InteractedCommand.start_notify_checkin:
                            commandResult = await registerMessageService.RemoveIgnoredEventMessage(turnContext.Activity.Conversation.Id, (int)MessageCategoryEnum.NotifyCheckIn);
                            if (commandResult.IsSuccessful)
                            {
                                var msgResponse = "Resgiter receive checkin notification successfully! ✅";
                                await turnContext.SendActivityAsync(MessageFactory.Text(msgResponse, msgResponse), cancellationToken);
                            }
                            break;

                        case "checkin_checkout_form":
                            var att = await formAdaptiveCardService.GetCheckInCheckOutAdaptiveCardForm(turnContext.Activity.Recipient.Id, turnContext.Activity.Conversation.Id);
                            // await messageService.SendAttachmentAsync("19:b376b8c7af164c0cba6fdd4f2788a05f@thread.v2", att);
                            IMessageActivity activity = att is not null ? MessageFactory.Attachment(att) : MessageFactory.Text("Something wrong! Please try again or check with admin");
                            await turnContext.SendActivityAsync(activity, cancellationToken);
                            break;

                        default:
                            await HandlerOtherMessages(turnContext, cancellationToken);
                            break;
                    }
                }
                else
                {
                    await HandlerSubmitData(turnContext);
                }
            }
        }

        protected override async Task OnUnrecognizedActivityTypeAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            logProviderService.LogFileMessage($"OnUnrecognizedActivityTypeAsync  -- Conversation Bot ID: {turnContext.Activity.Recipient.Id}. Conversation ID: {turnContext.Activity.From.Id}");
            var botName = turnContext.Activity.Recipient.Name;
            if (turnContext.Activity.Type == ActivityTypes.ContactRelationUpdate)
            {
                if (turnContext.Activity.Action == "add")
                {
                    await conversationService.AddConversationAsync(turnContext.Activity);
                    await turnContext.SendActivityAsync(MessageFactory.Text(string.Format(greetingMessage, botName)), cancellationToken);
                }
                else if (turnContext.Activity.Action == "remove")
                {
                    await conversationService.RemoveConversationAsync(turnContext.Activity);
                    await turnContext.SendActivityAsync(MessageFactory.Text("see you"), cancellationToken);
                }
            }
        }

        private async Task HandlerSubmitData(ITurnContext<IMessageActivity> turnContext)
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

                    case (int)SubmitAction.StopRemindRegistrationMeal:
                        await HandleSubmitStopRemindRegistrationMeal(turnContext);
                        break;

                    case (int)SubmitAction.StopCheckOutNotification:
                        await HandleStopReceiveNotificationByType(turnContext, MessageCategoryEnum.NotifyCheckOut, SubmitAction.StopCheckOutNotification);
                        break;

                    case (int)SubmitAction.StopReachedWorkingHoursNotification:
                        await HandleStopReceiveNotificationByType(turnContext, MessageCategoryEnum.ReminderReachedWorkingHour, SubmitAction.StopReachedWorkingHoursNotification);
                        break;

                    case (int)SubmitAction.StopCheckInNotification:
                        await HandleStopReceiveNotificationByType(turnContext, MessageCategoryEnum.NotifyCheckIn, SubmitAction.StopCheckInNotification);
                        break;

                    case (int)SubmitAction.ApproveMultipleRequests:
                        await HandleSubmitProcessMultipleRequests(turnContext, submitData);
                        break;

                    case (int)SubmitAction.StartStopMessageCategory:
                        await HandleStartStopMessageCategory(turnContext, JsonConvert.SerializeObject(submitData.ResponseData));
                        break;

                    case (int)SubmitAction.CommandMenu:
                        await HandleCommand(turnContext, JsonConvert.SerializeObject(submitData.ResponseData));
                        break;

                    case (int)SubmitAction.SubmitForm:
                        await HandleSubmitForm(turnContext, submitData.FormId, turnContext.Activity.Value);
                        break;

                    default:
                        break;
                }
            }
        }

        private async Task HandleSubmitForm(ITurnContext<IMessageActivity> turnContext, int formId, object dataValue)
        {
            switch (formId)
            {
                case 1:
                    var data = JsonConvert.DeserializeObject<CheckInCheckOutFormData>(dataValue.ToString());
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

            var msg = commandResult.IsSuccessful ?
                    $"✅ **{data.Command}** receive message category _{((MessageCategoryEnum)data.CategoryId).Description()}_ successfully! Type _help_ to get more information" :
                    $"❌ **{data.Command}** receive message category _{((MessageCategoryEnum)data.CategoryId).Description()}_ failed! Please contact admin";
            var activity = MessageFactory.Text(msg, msg);
            await turnContext.SendActivityAsync(activity);
        }

        private async Task HandleStopReceiveNotificationByType(ITurnContext<IMessageActivity> turnContext, MessageCategoryEnum messageCategory, SubmitAction submitAction)
        {
            var commandResult = await registerMessageService.IgnoreEventMessage(turnContext.Activity.Conversation.Id, (int)messageCategory);
            if (commandResult.IsSuccessful)
            {
                var msg = $"Ignored message category _{submitAction.Description()}_ successfully! Type _help_ to get more information";
                var activity = MessageFactory.Text(msg, msg);
                await turnContext.SendActivityAsync(activity);
            }
        }

        private async Task AddConversationAsync(ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var activity = turnContext.Activity;
            var info = await TeamsInfo.GetMemberAsync(turnContext, activity.From.Id, cancellationToken);
            var isGroup = activity.Conversation.IsGroup == true;
            string email = !isGroup ? info.Email : string.Empty;

            var command = new InsertConversationCommand
            {
                ConversationId = activity.Conversation.Id,
                ChannelId = activity.ChannelId,
                ServiceUrl = activity.ServiceUrl,
                UserName = email,
                IsGroup = activity.Conversation.IsGroup,
                IsActive = true,
                IsAdmin = false,
                ConversationBotId = activity.Recipient.Id,
                ConversationBotName = activity.Recipient.Name
            };

            await conversationService.AddConversationAsync(command);
            string botName = activity.Recipient.Name;

            if (!isGroup && !botName.Equals(botNameIgnoreMsg))
            {
                var msg = new StringBuilder($"Hi **{info.Name}**. My name is **{botName}**. ");
                msg.AppendLine("By added me, I will notify when there are events related to you such as requests that need your approval (Supervisors), requests that have been approved (Members). In addition, I also have a feature to remind you on the evaluation period or simplely when you reach the company's rules.");
                msg.AppendLine();
                msg.AppendLine(MessageConstants.HelpCommand(botName, false));
                var msgActivity = MessageFactory.Text(msg.ToString());
                msgActivity.TextFormat = "markdown";
                await turnContext.SendActivityAsync(msgActivity, cancellationToken);
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

        private async Task HandleSubmitStopRemindRegistrationMeal(ITurnContext<IMessageActivity> turnContext)
        {
            var commandResult = await registerMessageService.IgnoreEventMessage(turnContext.Activity.Conversation.Id, (int)MessageCategoryEnum.ReminderVegetarianMeal);
            if (commandResult.IsSuccessful)
            {
                await turnContext.SendActivityAsync(MessageFactory.Text(commandResult.Message, commandResult.Message));
            }
        }

        private async Task HandlerOtherMessages(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            try
            {
                var trimMessage = turnContext.Activity.Text.Trim();

                if (turnContext.Activity.Conversation.IsGroup == true)
                {
                    trimMessage = BotHelper.GetMessageInGroupByChannel(trimMessage, turnContext.Activity.ChannelId, turnContext.Activity.Recipient.Name);
                }

                IList<CategoryMessage> eventCategories;
                eventCategories = await registerMessageService.GetAllCategoryMessage(turnContext.Activity.Conversation.Id);
                if (trimMessage.StartsWith("Remove"))
                {
                    var category = eventCategories.FirstOrDefault(t => t.RemoveCommand.Equals(trimMessage));
                    if (category != null)
                    {
                        var result = await registerMessageService.RemoveRegisteredMessage(turnContext.Activity.Conversation.Id, category.CategoryId);
                        await turnContext.SendActivityAsync(MessageFactory.Text(result.Message, result.Message), cancellationToken);
                    }
                }
                else if (eventCategories.Any(t => t.RegisterCommand.Equals(trimMessage)))
                {
                    var category = eventCategories.FirstOrDefault(t => t.RegisterCommand.Equals(trimMessage));
                    if (category != null)
                    {
                        var result = await registerMessageService.RegisterEventMessage(turnContext.Activity.Conversation.Id, category.CategoryId);
                        await turnContext.SendActivityAsync(MessageFactory.Text(result.Message, result.Message), cancellationToken);
                    }
                }
                else
                {
                    var replyMsg = MessageFactory.Text(defaultReplyMessage);
                    replyMsg.TextFormat = "markdown";
                    await turnContext.SendActivityAsync(replyMsg, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                await logProviderService.SentryErrorAsync(ex.Message, ex);
                await messageService.AddSystemLog(ex.Message);
            }
        }
    }
}