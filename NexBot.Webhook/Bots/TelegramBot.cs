namespace NexBot.Bots
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Schema;
    using NexBot.Bots.Interfaces;
    using NexBot.Core;
    using NexBot.Core.Models;
    using NexBot.Core.Services.Interfaces;
    using NexBot.Core.Utilities;
    using NexBot.Models.CategoryMessage;
    using NexBot.Services.Interfaces;

    public class TelegramBot : ActivityHandler, ITelegramBot
    {
        private const string greetingMessage = "Hello, I am {0}";

        private readonly IMessageService messageService;
        private readonly IRequestService requestService;
        private readonly IRegistrationMealService registrationMealService;
        private readonly IRegisterMessageService registerMessageService;
        private readonly ILogProviderService logProviderService;
        protected readonly IConversationService conversationService;

        public TelegramBot(IMessageService messageService,
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

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var botName = turnContext.Activity.Recipient.Name;
            var message = turnContext.Activity.Text?.Trim() ?? string.Empty;

            if (message.StartsWith("/start"))
            {
                await conversationService.AddConversationAsync(turnContext.Activity);
                await turnContext.SendActivityAsync(MessageFactory.Text(string.Format(greetingMessage, botName)), cancellationToken);
            }

            if (turnContext.Activity.Conversation.IsGroup == true)
            {
                message = BotHelper.GetMessageInGroupByChannel(message, turnContext.Activity.ChannelId, botName);
                var channelData = turnContext.Activity.GetChannelData<TelegramData>();
                if (channelData.message.new_chat_member?.is_bot is true)
                {
                    await conversationService.AddConversationAsync(turnContext.Activity);
                }
                else if (channelData.message.left_chat_member?.is_bot is true)
                {
                    await conversationService.RemoveConversationAsync(turnContext.Activity);
                }
            }

            CommandResult commandResult;
            switch (message)
            {
                case InteractedCommand.help:
                    await turnContext.SendActivityAsync(MessageFactory.Text(string.Format(greetingMessage, botName)), cancellationToken);
                    break;

                case InteractedCommand.getid:
                    await turnContext.SendActivityAsync(MessageFactory.Text(turnContext.Activity.Conversation.Id), cancellationToken);
                    break;

                case InteractedCommand.getbot:
                    var botInfo = $"BotId: {turnContext.Activity.Recipient.Id}, BotName: {botName}";
                    await turnContext.SendActivityAsync(MessageFactory.Text(botInfo), cancellationToken);
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
                    commandResult = await registerMessageService.IgnoreEventMessage(turnContext.Activity.Conversation.Id, (int)MessageCategoryEnum.NotifyCheckOut);
                    if (commandResult.IsSuccessful)
                    {
                        var msg = "Ignored checkout notification successfully! ✅";
                        var activity = MessageFactory.Text(msg, msg);
                        await turnContext.SendActivityAsync(activity, cancellationToken);
                    }
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
                    commandResult = await registerMessageService.IgnoreEventMessage(turnContext.Activity.Conversation.Id, (int)MessageCategoryEnum.NotifyCheckIn);
                    if (commandResult.IsSuccessful)
                    {
                        var msg = "Ignored checkin notification successfully! ✅";
                        var activity = MessageFactory.Text(msg, msg);
                        await turnContext.SendActivityAsync(activity, cancellationToken);
                    }
                    break;

                case InteractedCommand.start_notify_checkin:
                    commandResult = await registerMessageService.RemoveIgnoredEventMessage(turnContext.Activity.Conversation.Id, (int)MessageCategoryEnum.NotifyCheckIn);
                    if (commandResult.IsSuccessful)
                    {
                        var msgResponse = "Resgiter receive checkin notification successfully! ✅";
                        await turnContext.SendActivityAsync(MessageFactory.Text(msgResponse, msgResponse), cancellationToken);
                    }
                    break;

                default:
                    await HandlerOtherMessages(turnContext, cancellationToken);
                    break;
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

                if (trimMessage.StartsWith("cmdApproved"))
                {
                    var result = await requestService.ProcessRequest(trimMessage, Constants.ApproveRequestCommand);
                    await turnContext.SendActivityAsync(MessageFactory.Text(result), cancellationToken);
                }
                else if (trimMessage.StartsWith("cmdReject"))
                {
                    var result = await requestService.ProcessRequest(trimMessage, Constants.RejectRequestCommand);
                    await turnContext.SendActivityAsync(MessageFactory.Text(result), cancellationToken);
                }
                else if (trimMessage.StartsWith("cmdRegMeal"))
                {
                    var result = await registrationMealService.RegistVegetarian(turnContext.Activity.Conversation.Id, trimMessage);
                    await turnContext.SendActivityAsync(MessageFactory.Text(result), cancellationToken);
                }
                else
                {
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