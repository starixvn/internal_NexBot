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
    using NexBot.Core;
    using NexBot.Core.Services.Interfaces;
    using NexBot.Core.Utilities;
    using NexBot.Models.CategoryMessage;
    using NexBot.Services.Interfaces;

    public class SkypeBot : ISkypeBot
    {
        private readonly IMessageService messageService;
        private readonly IRequestService requestService;
        private readonly IRegistrationMealService registrationMealService;
        private readonly IRegisterMessageService registerMessageService;
        private readonly ILogProviderService logProviderService;
        protected readonly IConversationService conversationService;

        private const string channelSkypeId = "skype";

        public SkypeBot(IMessageService messageService,
             IRequestService requestService,
             IRegistrationMealService registrationMealService,
             IRegisterMessageService registerMessageService,
             ILogProviderService logProviderService,
             IConversationService conversationService)
        {
            this.messageService = messageService;
            this.requestService = requestService;
            this.registrationMealService = registrationMealService;
            this.registerMessageService = registerMessageService;
            this.conversationService = conversationService;
            this.logProviderService = logProviderService;
        }

        public async Task OnConversationUpdateActivityAsync(ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            if (turnContext.Activity.MembersAdded != null)
            {
                if (turnContext.Activity.MembersAdded.Any(m => m.Id == turnContext.Activity.Recipient?.Id))
                {
                    logProviderService.WriteLogFileInfo("Add to group");
                    await conversationService.AddConversation(turnContext.Activity);
                    var message = $"Hello {turnContext.Activity.From.Name}. I am {turnContext.Activity.Recipient.Name}. If you have any question please contact admin.";
                    await turnContext.SendActivityAsync(MessageFactory.Text(message, turnContext.Activity.Conversation.Id), cancellationToken);
                }
            }
            else
            {
                if (turnContext.Activity.MembersRemoved != null
                && turnContext.Activity.MembersRemoved.Any(m => m.Id == turnContext.Activity.Recipient?.Id))
                {
                    logProviderService.WriteLogFileInfo("Remove from group");

                    await conversationService.RemoveConversation(turnContext.Activity);
                }
            }
        }

        public async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            try
            {
                IList<CategoryMessage> eventCategories;
                var trimMessage = turnContext.Activity.Text.Trim();

                if (turnContext.Activity.Conversation.IsGroup == true)
                {
                    trimMessage = BotHelper.GetMessageInGroupByChannel(trimMessage, turnContext.Activity.ChannelId, turnContext.Activity.Recipient.Name);
                }
                logProviderService.WriteLogFileInfo($"OnMessageActivityAsync: {trimMessage}");
                logProviderService.WriteLogFileInfo(JsonConvert.SerializeObject(turnContext.Activity.From));
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
                else if (trimMessage.StartsWith("cmdRegistVegetarianMeal"))
                {
                    var result = await registrationMealService.RegistVegetarian(turnContext.Activity.Conversation.Id, trimMessage);
                    await turnContext.SendActivityAsync(MessageFactory.Text(result), cancellationToken);
                }
                else if (trimMessage.StartsWith("stop_remind_vegetarian_meal"))
                {
                    var result = await registerMessageService.IgnoreEventMessage(turnContext.Activity.Conversation.Id, (int)MessageCategoryEnum.ReminderVegetarianMeal);
                    if (result.IsSuccessful)
                    {
                        await turnContext.SendActivityAsync(MessageFactory.Text(result.Message, result.Message), cancellationToken);
                    }
                }
                else if (trimMessage.StartsWith("start_remind_vegetarian_meal"))
                {
                    var result = await registerMessageService.RemoveIgnoredEventMessage(turnContext.Activity.Conversation.Id, (int)MessageCategoryEnum.ReminderVegetarianMeal);
                    if (result.IsSuccessful)
                    {
                        await turnContext.SendActivityAsync(MessageFactory.Text(result.Message, result.Message), cancellationToken);
                    }
                }
                else if (trimMessage.StartsWith("getid"))
                {
                    await conversationService.AddConversation(turnContext.Activity);
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

        public async Task OnEventActivityAsync(ITurnContext<IEventActivity> turnContext, CancellationToken cancellationToken)
        {
            if (turnContext.Activity.ChannelId.Equals(channelSkypeId))
            {
                logProviderService.WriteLogFileInfo("OnEventActivityAsync");
                await conversationService.AddConversation(turnContext.Activity);
            }
        }

        public async Task OnUnrecognizedActivityTypeAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            logProviderService.WriteLogFileInfo("OnUnrecognizedActivityTypeAsync");
            await conversationService.AddConversation(turnContext.Activity);
            var message = $"Hello {turnContext.Activity.Conversation.Name}. I am {turnContext.Activity.Recipient.Name}. If you have any question please contact admin.";
            logProviderService.WriteLogFileInfo(JsonConvert.SerializeObject(turnContext.Activity.From));
            await turnContext.SendActivityAsync(MessageFactory.Text(message, turnContext.Activity.Conversation.Id), cancellationToken);
        }
    }
}
