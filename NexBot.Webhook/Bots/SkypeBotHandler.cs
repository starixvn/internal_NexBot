namespace NexBot.Bots
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Schema;
    using NexBot.Core;
    using NexBot.Core.Services.Interfaces;
    using NexBot.Core.Utilities;
    using NexBot.Models.CategoryMessage;
    using NexBot.Services.Interfaces;

    public class SkypeBotHandler : ActivityHandler
    {
        private readonly IMessageService messageService;
        private readonly IRequestService requestService;
        private readonly IRegisterMessageService registerMessageService;
        private readonly ILogProviderService logProviderService;
        protected readonly BotState conversationState;
        protected readonly IConversationService conversationService;

        public SkypeBotHandler(IMessageService messageService,
             IRequestService requestService,
             ConversationState conversationState,
             IRegisterMessageService registerMessageService,
             ILogProviderService logProviderService,
             IConversationService conversationService)
        {
            this.messageService = messageService;
            this.requestService = requestService;
            this.registerMessageService = registerMessageService;
            this.conversationState = conversationState;
            this.conversationService = conversationService;
            this.logProviderService = logProviderService;
        }

        protected override async Task OnUnrecognizedActivityTypeAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            logProviderService.WriteLogFileInfo("=======Type/Action/COnversationId=========");
            logProviderService.WriteLogFileInfo(turnContext.Activity.Type);
            logProviderService.WriteLogFileInfo(turnContext.Activity.Action);
            logProviderService.WriteLogFileInfo(turnContext.Activity.Conversation.Id);
            logProviderService.WriteLogFileInfo("=======End=============");

            if (turnContext.Activity.Type == ActivityTypes.ContactRelationUpdate)
            {
                if (turnContext.Activity.Action.Equals("add"))
                {
                    await conversationService.AddConversation(turnContext.Activity);
                    var message = $"Hello {turnContext.Activity.From.Name}. I am {turnContext.Activity.Recipient.Name}. If you have any question please contact admin.";
                    await turnContext.SendActivityAsync(MessageFactory.Text(message, turnContext.Activity.Conversation.Id), cancellationToken);
                }
                else
                {
                    await conversationService.RemoveConversation(turnContext.Activity);
                }
            }
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            try
            {
                IList<CategoryMessage> eventCategories;
                var trimMessage = turnContext.Activity.Text.Trim();

                if (turnContext.Activity.Conversation.IsGroup == true)
                {
                    trimMessage = BotHelper.GetMessageInGroupByChannel(trimMessage, turnContext.Activity.ChannelId, turnContext.Activity.Recipient.Name);
                }

                eventCategories = await registerMessageService.GetAllCategoryMessage(turnContext.Activity.Conversation.Id);
                if (trimMessage.StartsWith("Remove"))
                {
                    if (eventCategories.Any(t => t.RemoveCommand.Equals(trimMessage)))
                    {
                        var categoryId = eventCategories.FirstOrDefault(t => t.RemoveCommand.Equals(trimMessage)).CategoryId;
                        var result = await registerMessageService.RemoveRegisteredMessage(turnContext.Activity.Conversation.Id, categoryId);
                        await turnContext.SendActivityAsync(MessageFactory.Text(result.Message, result.Message), cancellationToken);
                    }
                }
                else if (trimMessage.StartsWith("cmdApprovedRequest"))
                {
                    await requestService.ProcessRequest(turnContext.Activity.Conversation.Id, trimMessage, "Approve");
                }
                else if (trimMessage.StartsWith("cmdRejectRequest"))
                {
                    await requestService.ProcessRequest(turnContext.Activity.Conversation.Id, trimMessage, "Reject");
                }
                else if (trimMessage.StartsWith("getid"))
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text(turnContext.Activity.Conversation.Id, turnContext.Activity.Conversation.Id), cancellationToken);
                }
                else if (trimMessage.StartsWith("stop_remind_reachedworkingtime"))
                {
                    var result = await registerMessageService.IgnoreEventMessage(turnContext.Activity.Conversation.Id, (int)MessageCategoryEnum.ReminderReachedWorkingHour);
                    if (result.IsSuccessful)
                    {
                        await turnContext.SendActivityAsync(MessageFactory.Text(result.Message, result.Message), cancellationToken);
                    }
                }
                else if (trimMessage.StartsWith("start_remind_reachedworkingtime"))
                {
                    var result = await registerMessageService.RemoveIgnoredEventMessage(turnContext.Activity.Conversation.Id, (int)MessageCategoryEnum.ReminderReachedWorkingHour);
                    if (result.IsSuccessful)
                    {
                        await turnContext.SendActivityAsync(MessageFactory.Text(result.Message, result.Message), cancellationToken);
                    }
                }
                else
                {
                    if (eventCategories.Any(t => t.RegisterCommand.Equals(trimMessage)))
                    {
                        var categoryId = eventCategories.FirstOrDefault(t => t.RegisterCommand.Equals(trimMessage)).CategoryId;
                        var result = await registerMessageService.RegisterEventMessage(turnContext.Activity.Conversation.Id, categoryId);

                        await turnContext.SendActivityAsync(MessageFactory.Text(result.Message, result.Message), cancellationToken);
                    }
                }
            }
            catch (Exception ex)
            {
                await messageService.AddSystemLog(ex.Message);
            }
        }
    }
}