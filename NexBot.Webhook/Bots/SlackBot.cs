using Newtonsoft.Json;

namespace NexBot.Bots
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Schema;
    using NexBot.Bots.Interfaces;
    using NexBot.Core;
    using NexBot.Core.Services.Interfaces;
    using NexBot.Core.Utilities;
    using NexBot.Models.Slack;
    using NexBot.Services.Interfaces;

    public class SlackBot(
        IMessageService messageService,
        IRegisterMessageService registerMessageService,
        ILogProviderService logProviderService,
        IConversationService conversationService,
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
                            BotName = turnContext.Activity.Recipient.Name ?? "Bot",
                            UserId = slackData.SlackMessage.Event.User,
                            NickName = turnContext.Activity.From.Name.FormatNickName(),
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
            var botName = turnContext.Activity.Recipient?.Name ?? string.Empty;
            var message = turnContext.Activity.Text?.Trim() ?? string.Empty;

            if (turnContext.Activity.Conversation.IsGroup == true)
            {
                message = BotHelper.GetMessageInGroupByChannel(message, turnContext.Activity.ChannelId, botName);
            }

            if (botName.Equals(BotNameIgnoreMsg))
            {
                if (message.Equals(InteractedCommand.getid, StringComparison.OrdinalIgnoreCase))
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text(turnContext.Activity.Recipient.Id), cancellationToken);
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(message))
                {
                    switch (message.ToLowerInvariant())
                    {
                        case InteractedCommand.help:
                            var msgHelp = MessageFactory.Text(MessageConstants.HelpCommand(botName, false));
                            msgHelp.TextFormat = "markdown";
                            await turnContext.SendActivityAsync(msgHelp, cancellationToken);
                            break;

                        case InteractedCommand.getid:
                            var replyMsg = $"Your conversationId: {turnContext.Activity.Conversation.Id}";
                            await turnContext.SendActivityAsync(MessageFactory.Text(replyMsg), cancellationToken);
                            break;

                        case InteractedCommand.getbot:
                            var botInfo = $"BotId: {turnContext.Activity.Recipient.Id}, BotName: {botName}";
                            await turnContext.SendActivityAsync(MessageFactory.Text(botInfo), cancellationToken);
                            break;

                        default:
                            await HandlerOtherMessages(turnContext, cancellationToken);
                            break;
                    }
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
    }

}