namespace NexBot.Bots
{
    using System;
    using System.Linq;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Schema;
    using NexBot.Bots.Interfaces;
    using NexBot.Core;
    using NexBot.Core.Services.Interfaces;
    using NexBot.Core.Utilities;
    using NexBot.Criteria.Conversation;
    using NexBot.Models.Slack;
    using NexBot.Services.Helpers;
    using NexBot.Services.Interfaces;
    using Newtonsoft.Json;

#pragma warning disable S107 // Methods should not have too many parameters
    public class SlackBot(
        IMessageService messageService,
        IRegisterMessageService registerMessageService,
        ILogProviderService logProviderService,
        IConversationService conversationService,
        IFormAdaptiveCardService formAdaptiveCardService,
        IAccountService accountService,
        ISlackBotHandler slackBotHandler,
        IHRMConnector hrmConnector)
        : ActivityHandler, ISlackBot
#pragma warning restore S107 // Methods should not have too many parameters
    {
        private const string HraChannelId = "B08J3UJ8WMA:T08ACMHMMFS:C09KS77V3SN";
        private const string HrAssistantBotName = "hr_assistant";

        protected override async Task OnConversationUpdateActivityAsync(ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var channelData = turnContext.Activity.ChannelData?.ToString();
            if (!string.IsNullOrEmpty(channelData))
            {
                try
                {
                    var slackData = JsonConvert.DeserializeObject<SlackEventPayload>(channelData);
                    if (slackData.SlackMessage.Event.Type == "app_home_opened")
                    {
                        var payload = new SlackAppHomeViewPayload
                        {
                            UserId = slackData.SlackMessage.Event.User,
                            ApiToken = slackData.ApiToken
                        };
                        await slackBotHandler.PublishAppHomeView(payload);
                        await conversationService.AddConversationAsync(turnContext.Activity);
                    }
                }
                catch (JsonException ex)
                {
                    await logProviderService.SentryErrorAsync($"Failed to deserialize ChannelData: {ex.Message}", ex);
                }
            }
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var botId = turnContext.Activity.Recipient?.Id ?? string.Empty;
            var botName = turnContext.Activity.Recipient?.Name ?? string.Empty;
            var message = turnContext.Activity.Text?.Trim() ?? string.Empty;
            var conversationId = turnContext.Activity.Conversation.Id;
            var attachments = turnContext.Activity.Attachments;

            if (turnContext.Activity.Conversation.IsGroup == true)
            {
                message = BotHelper.GetMessageInGroupByChannel(message, turnContext.Activity.ChannelId, botName);
            }

            if (botName.Equals(HrAssistantBotName, StringComparison.OrdinalIgnoreCase))
            {
                await HandleN8NWebhookAsync(turnContext, message);
                return;
            }

            if (!string.IsNullOrEmpty(message))
            {
                switch (message.ToLowerInvariant())
                {
                    case InteractedCommand.getid:
                        var replyMsg = $"Your conversationId: {conversationId}";
                        await turnContext.SendActivityAsync(MessageFactory.Text(replyMsg), cancellationToken);
                        break;

                    case InteractedCommand.getbot:
                        var botInfo = $"BotId: {botId}, BotName: {botName}";
                        await turnContext.SendActivityAsync(MessageFactory.Text(botInfo), cancellationToken);
                        break;

                    case InteractedCommand.regist:
                        await AddConversationAsync(turnContext, cancellationToken);
                        break;

                    case InteractedCommand.regist_category:
                        await HandleRegistCategoryCommandAsync(conversationId);
                        break;

                    case InteractedCommand.checkin_checkout_form:
                        await HandleCheckInCheckOutFormCommandAsync(botId, conversationId);
                        break;

                    default:
                        await HandlerOtherMessages(turnContext, cancellationToken);
                        break;
                }
                return;
            }

            if (attachments != null && attachments.Any())
            {
                await HandlerOtherMessages(turnContext, cancellationToken);
            }
        }

        private async Task AddConversationAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var activity = turnContext.Activity;
            var channelData = activity.ChannelData?.ToString();
            if (!string.IsNullOrEmpty(channelData))
            {
                try
                {
                    SlackEventPayload slackData = JsonConvert.DeserializeObject<SlackEventPayload>(channelData);
                    var isGroup = IsGroupChannel(slackData.SlackMessage.Event.Channel);

                    var command = new InsertConversationCommand
                    {
                        ConversationId = activity.Conversation.Id,
                        ChannelId = activity.ChannelId,
                        ServiceUrl = activity.ServiceUrl,
                        UserName = isGroup ? activity.Conversation.Name : activity.From.Name,
                        IsGroup = isGroup,
                        IsActive = true,
                        IsAdmin = false,
                        ConversationBotId = activity.Recipient.Id,
                        ConversationBotName = activity.Recipient.Name
                    };

                    await conversationService.AddConversationAsync(command);
                    await turnContext.SendActivityAsync(MessageFactory.Text("Successfully!"), cancellationToken);
                }
                catch (JsonException ex)
                {
                    await logProviderService.SentryErrorAsync($"Failed to deserialize ChannelData: {ex.Message}", ex);
                }
            }
        }

        private async Task HandlerOtherMessages(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            try
            {
                var trimMessage = turnContext.Activity.Text.Trim();
                var attachments = turnContext.Activity.Attachments;
                var fromName = turnContext.Activity.From.Name;

                if (turnContext.Activity.Conversation.IsGroup == true)
                {
                    trimMessage = BotHelper.GetMessageInGroupByChannel(trimMessage, turnContext.Activity.ChannelId, turnContext.Activity.Recipient.Name);
                }

                var eventCategories = await registerMessageService.GetAllCategoryMessage(turnContext.Activity.Conversation.Id);
                if (trimMessage.StartsWith("Remove"))
                {
                    var category = eventCategories.FirstOrDefault(t => t.RemoveCommand.Equals(trimMessage));
                    if (category != null)
                    {
                        var result = await registerMessageService.RemoveRegisteredMessage(turnContext.Activity.Conversation.Id, category.CategoryId);
                        await turnContext.SendActivityAsync(MessageFactory.Text(result.Message), cancellationToken);
                    }
                }
                else if (eventCategories.Any(t => t.RegisterCommand.Equals(trimMessage)))
                {
                    var category = eventCategories.FirstOrDefault(t => t.RegisterCommand.Equals(trimMessage));
                    if (category != null)
                    {
                        var result = await registerMessageService.RegisterEventMessage(turnContext.Activity.Conversation.Id, category.CategoryId);
                        await turnContext.SendActivityAsync(MessageFactory.Text(result.Message), cancellationToken);
                    }
                }
                else if (turnContext.Activity.Conversation.Id == HraChannelId)
                {
                    var responseMessage = await messageService.SendMessageFromHRToUserAsync(fromName, turnContext.Activity.Text.Trim(), accountService, attachments);

                    if (!string.IsNullOrEmpty(responseMessage))
                    {
                        await turnContext.SendActivityAsync(MessageFactory.Text(responseMessage), cancellationToken);
                    }
                }
                else
                {
                    // Ignore message from other channels
                    if (turnContext.Activity.Conversation.Id.StartsWith("B08J3UJ8WMA:T08ACMHMMFS:C"))
                    {
                        return;
                    }
                    await messageService.SendMessageFromUserToHRAsync(fromName, trimMessage, HraChannelId, attachments);
                }
            }
            catch (Exception ex)
            {
                await logProviderService.SentryErrorAsync(ex.Message, ex);
                await messageService.AddSystemLog(ex.Message);
            }
        }

        private bool IsGroupChannel(string channelId)
        {
            if (string.IsNullOrEmpty(channelId))
            {
                return false;
            }
            return channelId.StartsWith("C");
        }

        private async Task HandleRegistCategoryCommandAsync(string conversationId)
        {
            var block = SlackBlockKitHelper.GenerateSlackBlockKit(
                Path.Combine(".", "Resources", "SlackBlockKit", "RegistCategoryMessageCard.json"),
                new { ConversationId = conversationId });
            await messageService.SendBlockKitAsync(conversationId, block);
        }

        private async Task HandleCheckInCheckOutFormCommandAsync(string botId, string conversationId)
        {
            var block = await formAdaptiveCardService.GetCheckInCheckOutBlockKitForm(botId, "", conversationId);
            if (block != null)
            {
                await messageService.SendBlockKitAsync(conversationId, block);
            }
            else
            {
                await messageService.SendAsync(conversationId, "Something wrong! Please try again or check with admin");
            }
        }

        private async Task HandleN8NWebhookAsync(ITurnContext<IMessageActivity> turnContext, string message)
        {
            var channelData = turnContext.Activity.ChannelData?.ToString();
            if (!string.IsNullOrEmpty(channelData))
            {
                var slackData = JsonConvert.DeserializeObject<SlackEventPayload>(channelData);
                var userId = slackData.SlackMessage.Event.User;
                await hrmConnector.SendWebhookN8N(turnContext.Activity.Conversation.Id, userId, message);
            }
        }

    }

}