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
    using NexBot.Services.Interfaces;

    public class TeamsBot : TeamsActivityHandler, ITeamsBot
    {
        private const string greetingMessage = "Hello, I am {0}";

        private readonly IMessageService messageService;
        private readonly IRequestService requestService;
        private readonly IRegistrationMealService registrationMealService;
        private readonly IRegisterMessageService registerMessageService;
        private readonly ILogProviderService logProviderService;
        protected readonly IConversationService conversationService;

        public TeamsBot(IMessageService messageService,
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
            if (turnContext.Activity.Text is not null)
            {
                var message = turnContext.Activity.Text?.Trim();
                var botName = turnContext.Activity.Recipient?.Name;
                if (turnContext.Activity.Conversation.IsGroup == true)
                {
                    message = BotHelper.GetMessageInGroupByChannel(message, turnContext.Activity.ChannelId, botName);
                }

                CommandResult commandResult;
                switch (message)
                {
                    case InteractedCommand.help:
                        var msg = MessageFactory.Text(MessageConstants.HelpCommand(botName, turnContext.Activity.Conversation.IsGroup == true));
                        msg.TextFormat = "markdown";
                        await turnContext.SendActivityAsync(msg, cancellationToken);
                        break;

                    case InteractedCommand.getid:
                        await turnContext.SendActivityAsync(MessageFactory.Text(turnContext.Activity.Conversation.Id), cancellationToken);
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

        protected override async Task OnUnrecognizedActivityTypeAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
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

                    default:
                        break;
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

            if (!isGroup)
            {
                string botName = activity.Recipient.Name;
                var msg = new StringBuilder($"Hi {info.Name}");
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

        private async Task HandleSubmitRegistVegetarian(ITurnContext<IMessageActivity> turnContext, AdaptiveSubmit submitData)
        {
            var registVegetarianMealMessage = JsonConvert.DeserializeObject<RegistVegetarianMealMessage>(submitData.ResponseData.ToString());
            var result = await registrationMealService.RegistVegetarian(registVegetarianMealMessage);
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
            }
            catch (Exception ex)
            {
                await logProviderService.SentryErrorAsync(ex.Message, ex);
                await messageService.AddSystemLog(ex.Message);
            }
        }
    }
}