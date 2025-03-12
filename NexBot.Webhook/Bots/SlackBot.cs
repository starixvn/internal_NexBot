namespace NexBot.Bots
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Schema;
    using Newtonsoft.Json;
    using NexBot.Bots.Interfaces;
    using NexBot.Core;
    using NexBot.Core.Services.Interfaces;
    using NexBot.Core.Utilities;
    using NexBot.Models.CategoryMessage;
    using NexBot.Services.Interfaces;

    public class SlackBot : ActivityHandler, ISlackBot
    {
        private readonly IMessageService _messageService;
        private readonly IRegisterMessageService _registerMessageService;
        private readonly ILogProviderService _logProviderService;
        private readonly IConversationService _conversationService;
        private readonly IFormAdaptiveCardService _formAdaptiveCardService;

        private const string GreetingMessage = "Hello, I am {0}";
        private const string DefaultReplyMessage = "Not supported command, type **help** to view available supported commands. Thanks & have a nice day";
        private const string BotNameIgnoreMsg = "Terra-Bot";

        public SlackBot(
            IMessageService messageService,
            IRegisterMessageService registerMessageService,
            ILogProviderService logProviderService,
            IConversationService conversationService,
            IFormAdaptiveCardService formAdaptiveCardService)
        {
            _messageService = messageService;
            _registerMessageService = registerMessageService;
            _logProviderService = logProviderService;
            _conversationService = conversationService;
            _formAdaptiveCardService = formAdaptiveCardService;
        }

        protected override async Task OnConversationUpdateActivityAsync(ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            if (turnContext.Activity.MembersAdded != null)
            {
                if (turnContext.Activity.MembersAdded.Any(m => m.Id == turnContext.Activity.Recipient?.Id))
                {
                    await _conversationService.AddConversationAsync(turnContext.Activity);
                }
            }
            else
            {
                if (turnContext.Activity.MembersRemoved != null
                    && turnContext.Activity.MembersRemoved.Any(m => m.Id == turnContext.Activity.Recipient?.Id))
                {
                    await _conversationService.RemoveConversationAsync(turnContext.Activity);
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
                        case "test":
                            await turnContext.SendActivityAsync(MessageFactory.Text(JsonConvert.SerializeObject(turnContext.Activity.From.Name)), cancellationToken);
                            break;
                        case InteractedCommand.help:
                            var attachment = await _messageService.GetAttachmentHelpAdaptiveCard(
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
                            await _conversationService.AddConversationAsync(turnContext.Activity);
                            break;

                        case InteractedCommand.checkin_checkout_form:
                            var att = await _formAdaptiveCardService.GetCheckInCheckOutAdaptiveCardForm(turnContext.Activity.Recipient.Id, turnContext.Activity.Conversation.Id);
                            IMessageActivity activity = att is not null ? MessageFactory.Attachment(att) : MessageFactory.Text("Something wrong! Please try again or check with admin");
                            await turnContext.SendActivityAsync(activity, cancellationToken);
                            break;

                        default:
                            await HandlerOtherMessages(turnContext, cancellationToken);
                            break;
                    }
                }
            }
        }

        protected override async Task OnUnrecognizedActivityTypeAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            _logProviderService.LogFileMessage($"OnUnrecognizedActivityTypeAsync  -- Conversation Bot ID: {turnContext.Activity.Recipient.Id}. Conversation ID: {turnContext.Activity.From.Id}");
            var botName = turnContext.Activity.Recipient.Name;

            if (turnContext.Activity.Type == ActivityTypes.Event)
            {
                switch (turnContext.Activity.Name)
                {
                    case "team_join":
                    case "member_joined_channel":
                        await _conversationService.AddConversationAsync(turnContext.Activity);
                        await turnContext.SendActivityAsync(MessageFactory.Text(string.Format(GreetingMessage, botName)), cancellationToken);
                        break;

                    case "channel_left":
                    case "member_left_channel":
                        await _conversationService.RemoveConversationAsync(turnContext.Activity);
                        await turnContext.SendActivityAsync(MessageFactory.Text("See you!"), cancellationToken);
                        break;

                    default:
                        _logProviderService.LogFileMessage($"Unhandled event: {turnContext.Activity.Name}");
                        break;
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

                IList<CategoryMessage> eventCategories = await _registerMessageService.GetAllCategoryMessage(turnContext.Activity.Conversation.Id);
                if (trimMessage.StartsWith("Remove"))
                {
                    var category = eventCategories.FirstOrDefault(t => t.RemoveCommand.Equals(trimMessage));
                    if (category != null)
                    {
                        var result = await _registerMessageService.RemoveRegisteredMessage(turnContext.Activity.Conversation.Id, category.CategoryId);
                        await turnContext.SendActivityAsync(MessageFactory.Text(result.Message), cancellationToken);
                    }
                }
                else if (eventCategories.Any(t => t.RegisterCommand.Equals(trimMessage)))
                {
                    var category = eventCategories.FirstOrDefault(t => t.RegisterCommand.Equals(trimMessage));
                    if (category != null)
                    {
                        var result = await _registerMessageService.RegisterEventMessage(turnContext.Activity.Conversation.Id, category.CategoryId);
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
                await _logProviderService.SentryErrorAsync(ex.Message, ex);
                await _messageService.AddSystemLog(ex.Message);
            }
        }
    }

}