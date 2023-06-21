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

    public class TelegramBot : ITelegramBot
    {
        private readonly IMessageService messageService;
        private readonly IRequestService requestService;
        private readonly IRegistrationMealService registrationMealService;
        private readonly IRegisterMessageService registerMessageService;
        private readonly ILogProviderService logProviderService;
        protected readonly IConversationService conversationService;

        private const string channelSkypeId = "skype";
        private const string greetingMessage = "Hello, I am {0}";

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

        public async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            try
            {
                IList<CategoryMessage> eventCategories;
                var trimMessage = turnContext.Activity.Text?.Trim() ?? string.Empty;

                if (trimMessage.StartsWith("/start"))
                {
                    await conversationService.AddConversation(turnContext.Activity);
                    string msg = string.Format("Hi {0}, I am {1}", turnContext.Activity.From.Name, turnContext.Activity.Recipient.Name);
                    await turnContext.SendActivityAsync(MessageFactory.Text(msg), cancellationToken);
                }

                if (turnContext.Activity.Conversation.IsGroup == true)
                {
                    trimMessage = BotHelper.GetMessageInGroupByChannel(trimMessage, turnContext.Activity.ChannelId, turnContext.Activity.Recipient.Name);

                    //// check add/remove bot 
                    ////var channelData = turnContext.Activity.GetChannelData<TelegramData>();
                    ////if (channelData.message.new_chat_member?.is_bot is true)
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
    }

    ////public class TelegramData
    ////{
    ////    public int update_id { get; set; }
    ////    public Message message { get; set; }
    ////}

    ////public class Message
    ////{
    ////    public int message_id { get; set; }
    ////    public From from { get; set; }
    ////    public Chat chat { get; set; }

    ////    public Member new_chat_member { get; set; }

    ////    public Member left_chat_member { get; set; }

    ////    public int date { get; set; }
    ////    public string text { get; set; }
    ////}

    ////public class From
    ////{
    ////    public int id { get; set; }
    ////    public bool is_bot { get; set; }
    ////    public string first_name { get; set; }
    ////    public string last_name { get; set; }
    ////    public string username { get; set; }
    ////    public string language_code { get; set; }
    ////}

    ////public class Chat
    ////{
    ////    public int id { get; set; }
    ////    public string first_name { get; set; }
    ////    public string last_name { get; set; }
    ////    public string username { get; set; }
    ////    public string type { get; set; }
    ////}

    ////public class Member
    ////{
    ////    public int id { get; set; }
    ////    public bool is_bot { get; set; }
    ////    public string first_name { get; set; }
    ////    public string username { get; set; }
    ////}
}
