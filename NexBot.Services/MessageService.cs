namespace NexBot.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AdaptiveCards.Templating;
    using Microsoft.Bot.Builder.Integration.AspNet.Core;
    using Microsoft.Bot.Schema;
    using Newtonsoft.Json;
    using NexBot.Core;
    using NexBot.Core.Services.Interfaces;
    using NexBot.Core.Utilities;
    using NexBot.Criteria;
    using NexBot.Criteria.Log;
    using NexBot.Criteria.Message;
    using NexBot.Criteria.RegisterMessage;
    using NexBot.Models;
    using NexBot.Models.CategoryMessage;
    using NexBot.Models.Request;
    using NexBot.Services.Helpers;
    using NexBot.Services.Interfaces;

    public class MessageService(
        IBaseService baseService,
        IBotFrameworkHttpAdapter adapter,
        IConversationService conversationService,
        ILogProviderService logProviderService) : IMessageService
    {
        public async Task AddSystemLog(string logContent)
        {
            var criteria = new InsertSystemLogCommand
            {
                LogContent = logContent
            };

            await baseService.ExecuteNonQueryAsync(criteria);
        }

        public async Task ReSendErrorMeesage()
        {
            var messages = await GetMessageLog();

            foreach (var message in messages)
            {
                await ReSendMessage(message.ConversationId, message.MessageContent);
            }
        }

        private async Task ReSendMessage(string conversationId, string messageText)
        {
            var conversation = await conversationService.GetConversationById(conversationId);
            var conversationReference = GetConversationReference(conversation);
            var activity = CreateMessageActivity(conversationReference);
            try
            {
                if (!string.IsNullOrEmpty(messageText))
                {
                    activity.Text = messageText;
                    await SendMessageAsync(conversationReference, activity);
                    await AddMessageLog(activity, messageText);
                }
            }
            catch (Exception ex)
            {
                await AddSystemLog(ex.Message);
            }
        }

        public async Task SendAsync(string conversationId, string messageText)
        {
            logProviderService.LogFileMessage($"============= SendAsync: ConversationID: {conversationId}. Text = {messageText}==================");
            try
            {
                var conversationModel = await conversationService.GetConversationById(conversationId);
                if (conversationModel is null)
                {
                    logProviderService.LogFileMessage($"============= ConversationID: {conversationId}. does not existed ==================");
                }
                else
                {
                    var conversationInfo = GetConversationReference(conversationModel);
                    var activity = CreateMessageActivity(conversationInfo);
                    activity.Text = messageText;
                    await SendMessageAsync(conversationInfo, activity);
                    await AddMessageLog(activity, messageText);
                }
            }
            catch (Exception ex)
            {
                await logProviderService.SentryErrorAsync($"ConversationID: {conversationId}. Text = {messageText}", ex);
            }
        }

        private static ConversationReference GetConversationReference(ConversationModel conversationModel)
        {
            var conversation = new ConversationReference
            {
                Bot = new ChannelAccount { Id = conversationModel.ConversationBotId, Name = conversationModel.ConversationBotName, Role = "bot" },
                Conversation = new ConversationAccount { Id = conversationModel.ConversationId },
                ServiceUrl = conversationModel.ServiceUrl,
                ChannelId = conversationModel.ChannelId
            };
            return conversation;
        }

        private async Task AddErrorMessageLog(ConversationReference conversationReference, string message)
        {
            var criteria = new InsertErrorMessageLogCommand
            {
                ConversationId = conversationReference.Conversation.Id,
                FromConversationId = conversationReference.Bot.Id,
                FromName = conversationReference.Bot.Name,
                MessageContent = BotHelper.GenerateMessage(message)
            };

            await baseService.ExecuteNonQueryAsync(criteria);
        }

        public async Task SendRequestAsync(RequestMessage requestMessage)
        {
            logProviderService.LogFileMessage(JsonConvert.SerializeObject(requestMessage));
            try
            {
                var conversation = await conversationService.GetConversationById(requestMessage.ConversationId);
                var conversationReference = GetConversationReference(conversation);
                var activity = CreateMessageActivity(conversationReference);
                if (!string.IsNullOrEmpty(requestMessage.Message))
                {
                    Attachment attachment;
                    if (conversation.ChannelId.Equals(Constants.TeamsChannel))
                    {
                        attachment = GetAttachmentForRequestInTeams(requestMessage);
                    }
                    else if (conversation.ChannelId.Equals(Constants.SlackChannel))
                    {
                        // TODO
                        return;
                    }
                    else
                    {
                        if (requestMessage.IsReadOnly)
                        {
                            return;
                        }

                        activity.Text = requestMessage.Message;
                        attachment = GetAttachmentForRequest(requestMessage);
                    }

                    activity.Attachments.Add(attachment);
                    await SendMessageAsync(conversationReference, activity);
                    await AddMessageLog(activity, requestMessage.Message);
                }
            }
            catch (Exception ex)
            {
                await logProviderService.SentryErrorAsync(JsonConvert.SerializeObject(requestMessage), ex);
            }
        }

        private static Attachment GetAttachmentForRequestInTeams(RequestMessage requestMessage)
        {
            string fileName;
            if (requestMessage.RequestType.Equals("leave"))
            {
                fileName = requestMessage.IsReadOnly ? "LeaveRequestReadOnlyCard.json" : "LeaveRequestCard.json";
            }
            else
            {
                fileName = "CheckInOutRequestCard.json";
            }

            var filePath = Path.Combine(".", "Resources", fileName);
            var templateJson = File.ReadAllText(filePath);
            AdaptiveCardTemplate template = new(templateJson);
            string cardJson = template.Expand(requestMessage);
            var cardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(cardJson),
            };
            return cardAttachment;
        }

        private static Attachment GetAttachmentForRequest(RequestMessage requestMessage)
        {
            var approveCommand = string.Format("{0}_{1}_{2}_{3}", "cmdApproved", requestMessage.RequestId, requestMessage.ProcessedBy, requestMessage.CallBackUrl);
            var rejectCommand = string.Format("{0}_{1}_{2}_{3}", "cmdReject", requestMessage.RequestId, requestMessage.ProcessedBy, requestMessage.CallBackUrl);
            var cardActions = new List<CardAction>
                    {
                        new(ActionTypes.PostBack, title: "Approve", text: approveCommand, displayText: "Approve", value: approveCommand),
                        new(ActionTypes.PostBack, title: "Reject", text: rejectCommand, displayText: "Reject", value: rejectCommand)
                    };
            var heroCard = new HeroCard
            {
                Buttons = cardActions
            };

            var attachment = heroCard.ToAttachment();
            return attachment;
        }

        private static Attachment GetAttachmentForRegistMealInTeams(RegistVegetarianMealMessage registVegetarianMessage)
        {
            var fileName = "RegistVegetarianMealCard.json";
            var filePath = Path.Combine(".", "Resources", fileName);
            var templateJson = File.ReadAllText(filePath);
            AdaptiveCardTemplate template = new(templateJson);
            string cardJson = template.Expand(registVegetarianMessage);

            var cardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(cardJson),
            };
            return cardAttachment;
        }

        private static Attachment GetAttachmentForRegistMeal(RegistVegetarianMealMessage registVegetarianMessage)
        {
            var command = string.Format("{0}_{1}_{2}_{3}_{4}",
                "cmdRegMeal",
                registVegetarianMessage.EmpId,
                registVegetarianMessage.CompanyId,
                registVegetarianMessage.OnDate.ToString("yyyy-MM-dd"),
                registVegetarianMessage.CallBackUrl);

            var cardActions = new List<CardAction>
                    {
                        new(ActionTypes.PostBack, "Đặt Cơm Chay", value: command)
                    };
            var heroCard = new HeroCard
            {
                Buttons = cardActions
            };
            var attachment = heroCard.ToAttachment();
            return attachment;
        }

        public async Task RegistVegetarianMealAsync(RegistVegetarianMealMessage registVegetarianMessage)
        {
            var conversation = await conversationService.GetConversationById(registVegetarianMessage.ConversationId);
            var conversationReference = GetConversationReference(conversation);
            var activity = CreateMessageActivity(conversationReference);
            if (!string.IsNullOrEmpty(registVegetarianMessage.Message))
            {
                Attachment attachment;
                if (conversation.ChannelId.Equals(Constants.TeamsChannel))
                {
                    attachment = GetAttachmentForRegistMealInTeams(registVegetarianMessage);
                }
                else if (conversation.ChannelId.Equals(Constants.SlackChannel))
                {
                    // TODO
                    return;
                }
                else
                {
                    activity.Text = registVegetarianMessage.Message;
                    attachment = GetAttachmentForRegistMeal(registVegetarianMessage);
                }

                activity.Attachments.Add(attachment);

                await SendMessageAsync(conversationReference, activity);
                await AddMessageLog(activity, registVegetarianMessage.Message);
            }
        }

        public async Task SendAdminAsync(string message)
        {
            var conversations = await conversationService.GetAdminConversation();
            foreach (var conversation in conversations)
            {
                var adminConversationReferrence = GetConversationReference(conversation);
                var activity = CreateMessageActivity(adminConversationReferrence);
                activity.Text = message;
                await SendMessageAsync(adminConversationReferrence, activity);
            }
        }

        private static IMessageActivity CreateMessageActivity(ConversationReference conversation)
        {
            var message = Activity.CreateMessageActivity();
            message.ChannelId = conversation.ChannelId;
            message.From = new ChannelAccount
            {
                Id = conversation.Bot.Id,
                Name = conversation.Bot.Name
            };
            message.Recipient = new ChannelAccount
            {
                Id = conversation.Conversation.Id,
                Name = conversation.Conversation.Name
            };
            message.Conversation = new ConversationAccount(id: conversation.Conversation.Id);

            return message;
        }

        private async Task AddMessageLog(IMessageActivity activity, string messageContent)
        {
            var criteria = new InsertMessageLogCommand
            {
                ConversationId = activity.Conversation.Id,
                FromConversationId = activity.From.Id,
                FromName = activity.From.Name,
                MessageContent = GetLogContent(activity, messageContent)
            };

            await baseService.ExecuteNonQueryAsync(criteria);
        }

        private static string GetLogContent(IMessageActivity activity, string messageContent)
        {
            string logMsgContent = BotHelper.GenerateMessage(activity.Text);
            if (string.IsNullOrWhiteSpace(logMsgContent))
            {
                if (!string.IsNullOrWhiteSpace(messageContent))
                {
                    logMsgContent = messageContent;
                }
                else
                {
                    if (activity.ChannelId.Equals(Constants.TeamsChannel)
                        && activity.Attachments is not null && activity.Attachments.Any())
                    {
                        logMsgContent = JsonConvert.SerializeObject(activity.Attachments.FirstOrDefault());
                    }
                    else if (activity.ChannelId.Equals(Constants.SlackChannel) 
                        && activity.ChannelData is not null)
                    {
                        logMsgContent = JsonConvert.SerializeObject(activity.ChannelData);
                    }
                }
            }

            return logMsgContent;
        }

        private async Task<IList<Message>> GetMessageLog()
        {
            var criteria = new GetErrorMessageLogCriteria();

            var messages = await baseService.GetDataAsync<Message>(criteria);

            return messages.ToList();
        }

        private async Task SendMessageAsync(ConversationReference conversationReference, IActivity activity)
        {
            try
            {
                var bots = await conversationService.GetBotAsync();
                if (bots.Any(t => t.ChannelName == activity.ChannelId && t.BotName == activity.From.Name))
                {
                    var bot = bots.FirstOrDefault(t => t.ChannelName == activity.ChannelId && t.BotName == activity.From.Name);

                    string appId = bot?.MicrosoftAppId;
                    await ((CloudAdapter)adapter).ContinueConversationAsync(appId, conversationReference,
                        async (context, token) =>
                        {
                            await context.SendActivityAsync(activity, cancellationToken: token);
                        },
                        new CancellationToken());
                }
            }
            catch (Exception ex)
            {
                logProviderService.LogFileError(ex);

                await AddErrorMessageLog(conversationReference, activity.AsMessageActivity().Text);
                await AddSystemLog(ex.Message);
                await SendAdminAsync(
                    $"Error happen in client with ConversationId: {conversationReference.Conversation.Id} & Message: {activity.AsMessageActivity().Text}\n\n" +
                    $"Exception: {ex.Message}");
            }
        }

        public async Task CheckoutNotificationAsync(TimesheetRecord record)
           => await TimesheetRecordNotificationAsync(record, "NotifyCheckoutCard.json");

        public async Task CheckinNotificationAsync(TimesheetRecord record)
           => await TimesheetRecordNotificationAsync(record, "NotifyCheckinCard.json");

        private async Task TimesheetRecordNotificationAsync(TimesheetRecord record, string templateName)
        {
            var conversation = await conversationService.GetConversationById(record.ConversationId);
            if (conversation is not null)
            {
                var conversationReference = GetConversationReference(conversation);
                var activity = CreateMessageActivity(conversationReference);
                if (conversation.ChannelId.Equals(Constants.TeamsChannel))
                {
                    var attachment = AdaptiveCardHelper.GenerateAdaptiveCardTeamsChannel(
                        Path.Combine(".", "Resources", templateName),
                        record);
                    activity.Attachments.Add(attachment);
                }
                else if (conversation.ChannelId.Equals(Constants.SlackChannel))
                {
                    var block = SlackBlockKitHelper.GenerateSlackBlockKit(
                        Path.Combine(".", "Resources", templateName),
                        record);
                    activity.ChannelData = block;
                }
                else
                {
                    activity.Text = record.MessageContent;
                }

                await SendMessageAsync(conversationReference, activity);
                await AddMessageLog(activity, record.MessageContent);
            }
        }

        public async Task ReachedWorkingHoursAsync(ReachedWorkingHours reachedWorkingHours)
        {
            var conversation = await conversationService.GetConversationById(reachedWorkingHours.ConversationId);
            if (conversation is not null)
            {
                var conversationReference = GetConversationReference(conversation);
                var activity = CreateMessageActivity(conversationReference);
                if (conversation.ChannelId.Equals(Constants.TeamsChannel))
                {
                    var attachment = AdaptiveCardHelper.GenerateAdaptiveCardTeamsChannel(
                        Path.Combine(".", "Resources", "ReachedWorkingHoursCard.json"),
                        reachedWorkingHours);
                    activity.Attachments.Add(attachment);
                }
                else if (conversation.ChannelId.Equals(Constants.SlackChannel))
                {
                    var block = SlackBlockKitHelper.GenerateSlackBlockKit(
                        Path.Combine(".", "Resources", "ReachedWorkingHoursCard.json"),
                        reachedWorkingHours);
                    activity.ChannelData = block;
                }
                else
                {
                    activity.Text = reachedWorkingHours.MessageContent;
                    
                }

                await SendMessageAsync(conversationReference, activity);
                await AddMessageLog(activity, reachedWorkingHours.MessageContent);
            }
        }

        public async Task RemindApproveMultiplePendingRequests(PendingRequest request)
        {
            var conversation = await conversationService.GetConversationById(request.ConversationId);
            if (conversation is not null)
            {
                var conversationReference = GetConversationReference(conversation);
                var activity = CreateMessageActivity(conversationReference);
                if (conversation.ChannelId.Equals(Constants.TeamsChannel))
                {
                    Attachment attachment = AdaptiveCardHelper.GenerateAdaptiveCardTeamsChannel(
                         Path.Combine(".", "Resources", "ApproveMultipleRequestCard.json"),
                         request);
                    activity.Attachments.Add(attachment);
                    await SendMessageAsync(conversationReference, activity);
                    await AddMessageLog(activity, $"RemindPendingRequests: {request.ApproverNickname} - " + request.StringRequests);
                }
            }
        }

        public async Task RemoveOldMessageLog()
         => await baseService.ExecuteNonQueryAsync(new RemoveOldMessageLogCommand());

        public async Task<Attachment> GetAttachmentHelpAdaptiveCard(string conversationId, bool isGroup, string botName)
        {
            var criteria = new GetRegisterCategoryMessageCriteria
            {
                ConversationId = conversationId,
                IsGroup = isGroup
            };

            var registeredCategoryMessages = await baseService.GetDataAsync<RegisterCategoryMessage>(criteria);
            var model = new HelpMenuAdaptiveCard
            {
                BotName = botName,
                RegisterCategoryMessages = registeredCategoryMessages
            };

            Attachment attachment = AdaptiveCardHelper.GenerateAdaptiveCardTeamsChannel(
                         Path.Combine(".", "Resources", "HelpCard.json"),
                         model);

            return attachment;
        }

        public async Task SendAttachmentAsync(string conversationId, Attachment attachment)
        {
            try
            {
                var conversation = await conversationService.GetConversationById(conversationId);
                var conversationReference = GetConversationReference(conversation);
                var activity = CreateMessageActivity(conversationReference);

                activity.Attachments.Add(attachment);

                await SendMessageAsync(conversationReference, activity);
            }
            catch (Exception ex)
            {
                await AddSystemLog(ex.Message);
            }
        }

        public async Task SendBlockKitAsync(string conversationId, object block)
        {
            try
            {
                var conversation = await conversationService.GetConversationById(conversationId);
                var conversationReference = GetConversationReference(conversation);
                var activity = CreateMessageActivity(conversationReference);

                activity.ChannelData = block;

                await SendMessageAsync(conversationReference, activity);
            }
            catch (Exception ex)
            {
                await AddSystemLog(ex.Message);
            }
        }
    }
}