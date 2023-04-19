namespace NexBot.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Integration;
    using Microsoft.Bot.Connector.Authentication;
    using Microsoft.Bot.Schema;
    using Microsoft.Extensions.Configuration;
    using NexBot.Core.Services;
    using NexBot.Core.Utilities;
    using NexBot.Criteria;
    using NexBot.Criteria.Message;
    using NexBot.Models;
    using NexBot.Services.Interfaces;

    public class MessageService : BaseService, IMessageService
    {
        private readonly IConfiguration configuration;
        private readonly IAdapterIntegration adapter;
        private readonly IConversationService conversationService;

        public MessageService(
            IAdapterIntegration adapter,
            IConfiguration configuration,
            IConversationService conversationService)
        {
            this.configuration = configuration;
            this.adapter = adapter;
            this.conversationService = conversationService;
        }

        public async Task AddSystemLog(string logContent)
        {
            var criteria = new InsertSystemLogCriteria
            {
                LogContent = logContent
            };

            await ExecuteNonQueryAsync(criteria);
        }

        public async Task ReSendErrorMeesage()
        {
            var messages = await GetMessageLog();

            foreach (var message in messages)
            {
                await ReSendMessage(message.ConversationId, message.MessageContent);
            }
        }

        private async Task ReSendMessage(string conversationId, string messageText)
        {
            var conversation = await conversationService.GetConversationById(conversationId);
            var conversationReference = GetConversationReference(conversation);
            var activity = CreateMessageActivity(conversationReference);
            try
            {
                if (!string.IsNullOrEmpty(messageText))
                {
                    activity.Text = messageText;
                    await SendMessageAsync(conversationReference, activity);
                    await AddMessageLog(activity);
                }
            }
            catch (Exception ex)
            {
                await AddSystemLog(ex.Message);
            }
        }

        public async Task SendAsync(string conversationId, string messageText)
        {
            var conversationModel = await conversationService.GetConversationById(conversationId);
            var conversationInfo = GetConversationReference(conversationModel);
            var activity = CreateMessageActivity(conversationInfo);
            activity.Text = messageText;
            await SendMessageAsync(conversationInfo, activity);
            await AddMessageLog(activity);
        }

        private ConversationReference GetConversationReference(ConversationModel conversationModel)
        {
            var coversation = new ConversationReference
            {
                Bot = new ChannelAccount { Id = conversationModel.ConversationBotId, Name = conversationModel.ConversationBotName, Role = "bot" },
                Conversation = new ConversationAccount { Id = conversationModel.ConversationId },
                ServiceUrl = conversationModel.ServiceUrl,
                ChannelId = conversationModel.ChannelId
            };

            return coversation;
        }

        private async Task AddErrorMessageLog(ConversationReference conversationReference, string message)
        {
            var criteria = new InsertErrorMessageLogCriteria
            {
                ConversationId = conversationReference.Conversation.Id,
                FromConversationId = conversationReference.Bot.Id,
                FromName = conversationReference.Bot.Name,
                MessageContent = BotHelper.GenerateMessage(message)
            };

            await ExecuteNonQueryAsync(criteria);
        }

        public async Task SendRequestAsync(SendRequestMessage requestMessage)
        {
            var conversation = await conversationService.GetConversationById(requestMessage.ConversationId);
            var conversationReference = GetConversationReference(conversation);
            var activity = CreateMessageActivity(conversationReference);
            if (!string.IsNullOrEmpty(requestMessage.Message))
            {
                activity.Text = requestMessage.Message;
                var approveCommand = string.Format("{0}_{1}_{2}", "cmdApprovedRequest", requestMessage.RequestId.ToString(), requestMessage.CallBackUrl);
                var rejectCommand = string.Format("{0}_{1}_{2}", "cmdRejectRequest", requestMessage.RequestId.ToString(), requestMessage.CallBackUrl);
                var cardActions = new List<CardAction>
                    {
                        new CardAction(ActionTypes.PostBack, "Approve", value: approveCommand),
                        new CardAction(ActionTypes.PostBack, "Reject", value: rejectCommand)
                    };
                var heroCard = new HeroCard
                {
                    Buttons = cardActions
                };

                var attachment = heroCard.ToAttachment();
                activity.Attachments = new List<Attachment>()
                    {
                        attachment
                    };

                await SendMessageAsync(conversationReference, (Activity)activity);
                await AddMessageLog(activity);
            }
        }

        public async Task RegistVegetarianMealAsync(RegistVegetarianMealMessage message)
        {
            var conversation = await conversationService.GetConversationById(message.ConversationId);
            var conversationReference = GetConversationReference(conversation);
            var activity = CreateMessageActivity(conversationReference);
            if (!string.IsNullOrEmpty(message.Message))
            {
                activity.Text = message.Message;
                var registVegetarianCommand = string.Format("{0}_{1}_{2}_{3}_{4}", "cmdRegistVegetarianMeal", message.EmpId, message.CompanyId, message.OnDate.ToString("yyyy-MM-dd"), message.CallBackUrl);
                var cardActions = new List<CardAction>
                    {
                        new CardAction(ActionTypes.PostBack, "Đặt Cơm Chay", value: registVegetarianCommand)
                    };
                var heroCard = new HeroCard
                {
                    Buttons = cardActions
                };

                var attachment = heroCard.ToAttachment();
                activity.Attachments = new List<Attachment>()
                    {
                        attachment
                    };

                await SendMessageAsync(conversationReference, (Activity)activity);
                await AddMessageLog(activity);
            }
        }

        public async Task SendAdminAsync(string message)
        {
            var conversations = await conversationService.GetAdminConversation();
            foreach (var conversation in conversations)
            {
                var adminConversationReferrence = GetConversationReference(conversation);
                var activity = CreateMessageActivity(adminConversationReferrence);
                activity.Text = message;
                await SendMessageAsync(adminConversationReferrence, activity);
            }
        }

        private static IMessageActivity CreateMessageActivity(ConversationReference conversation)
        {
            var message = Activity.CreateMessageActivity();
            message.ChannelId = conversation.ChannelId;
            message.From = new ChannelAccount
            {
                Id = conversation.Bot.Id,
                Name = conversation.Bot.Name
            };
            message.Recipient = new ChannelAccount
            {
                Id = conversation.Conversation.Id,
                Name = conversation.Conversation.Name
            };
            message.Conversation = new ConversationAccount(id: conversation.Conversation.Id);

            return message;
        }

        private async Task AddMessageLog(IMessageActivity activity)
        {
            var criteria = new InsertMessageLogCriteria
            {
                ConversationId = activity.Conversation.Id,
                FromConversationId = activity.From.Id,
                FromName = activity.From.Name,
                MessageContent = BotHelper.GenerateMessage(activity.Text)
            };

            await ExecuteNonQueryAsync(criteria);
        }

        private async Task<IList<Message>> GetMessageLog()
        {
            var criteria = new GetErrorMessageLogCriteria();

            var messages = await GetDataAsync<Message>(criteria);

            return messages.ToList();
        }

        private async Task SendMessageAsync(ConversationReference conversationReference, IActivity activity)
        {
            try
            {
                string appId = configuration.GetSection("SkypeBotConfigurations:MicrosoftAppId")?.Value;
                await adapter.ContinueConversationAsync(appId, conversationReference,
                        async (context, token) =>
                        {
                            AppCredentials.TrustServiceUrl(conversationReference.ServiceUrl);
                            await context.SendActivityAsync(activity, token);
                        },
                        new CancellationToken());
            }
            catch (Exception ex)
            {
                await AddErrorMessageLog(conversationReference, activity.AsMessageActivity().Text);
                await AddSystemLog(ex.Message);
                await SendAdminAsync(
                    $"Error happen in client with ConversationId: {conversationReference.Conversation.Id} & Message: {activity.AsMessageActivity().Text}\n\n" +
                    $"Exception: {ex.Message}");
            }
        }
    }
}