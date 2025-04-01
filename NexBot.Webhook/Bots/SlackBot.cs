namespace NexBot.Bots
{
    using System;
    using System.IO;
    using System.Linq;
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

    public class SlackBot(
        IMessageService messageService,
        IRegisterMessageService registerMessageService,
        ILogProviderService logProviderService,
        IConversationService conversationService,
        IFormAdaptiveCardService formAdaptiveCardService,
        ISlackBotHandler slackBotHandler)
        : ActivityHandler, ISlackBot
    {
        private const string DefaultReplyMessage = "Not supported command, type **help** to view available supported commands. Thanks & have a nice day";
        private const string BotNameIgnoreMsg = "Terra-Bot";

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

            if (turnContext.Activity.Conversation.IsGroup == true)
            {
                message = BotHelper.GetMessageInGroupByChannel(message, turnContext.Activity.ChannelId, botName);
            }

            if (botName.Equals(BotNameIgnoreMsg))
            {
                if (message.Equals(InteractedCommand.getid, StringComparison.OrdinalIgnoreCase))
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text(botId), cancellationToken);
                }
            }
            else
            {
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

                        case InteractedCommand.help:
                            await turnContext.SendActivityAsync(MessageFactory.Text("This feature is coming soon!"), cancellationToken);
                            break;

                        case InteractedCommand.checkin_checkout_form:
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

                        default:
                            await HandlerOtherMessages(turnContext, cancellationToken);
                            break;
                    }
                }
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
                    var block = SlackBlockKitHelper.GenerateSlackBlockKit(
                        Path.Combine(".", "Resources", "SlackBlockKit", "RegistCategoryMessageCard.json"),
                        new { ConversationId = turnContext.Activity.Conversation.Id });
                    await messageService.SendBlockKitAsync(turnContext.Activity.Conversation.Id, block);
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
                else
                {
                    var replyMsg = MessageFactory.Text(DefaultReplyMessage);
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

        private bool IsGroupChannel(string channelId)
        {
            if (string.IsNullOrEmpty(channelId))
            {
                return false;
            }   
            return channelId.StartsWith("C");
        }
    }

}