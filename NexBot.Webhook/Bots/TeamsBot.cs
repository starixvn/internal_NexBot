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
    using NexBot.Bots.Interfaces;
    using NexBot.Core;
    using NexBot.Core.Services.Interfaces;
    using NexBot.Core.Utilities;
    using NexBot.Criteria.Conversation;
    using NexBot.Models.CategoryMessage;
    using NexBot.Services.Interfaces;

    public class TeamsBot(IMessageService messageService,
         IRegisterMessageService registerMessageService,
         ILogProviderService logProviderService,
         IConversationService conversationService,
         IFormAdaptiveCardService formAdaptiveCardService,
         ITeamsBotHandler teamsBotHandler) : TeamsActivityHandler, ITeamsBot
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

                        case "checkin_checkout_form":
                            var att = await formAdaptiveCardService.GetCheckInCheckOutAdaptiveCardForm(turnContext.Activity.Recipient.Id, turnContext.Activity.Conversation.Id);
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
                    await teamsBotHandler.HandleSubmitData(turnContext);
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