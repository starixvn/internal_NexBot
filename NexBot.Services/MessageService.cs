namespace NexBot.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Integration;
    using Microsoft.Bot.Connector;
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
            var connector = CreateConnectorClient(new Uri(conversation.ServiceUrl));
            var message = CreateMessageActivity(conversation);
            try
            {
                if (!string.IsNullOrEmpty(messageText))
                {
                    message.Text = messageText;
                    await connector.Conversations.SendToConversationAsync((Activity)message);
                    await AddMessageLog(message);
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

            await SendMessageAsync(conversationInfo, messageText);
        }

        private async Task SendMessageAsync(ConversationReference conversationReference, string message)
        {
            try
            {
                string appId = configuration.GetSection("SkypeBotConfigurations:MicrosoftAppId")?.Value;
                await adapter.ContinueConversationAsync(appId, conversationReference,
                        async (context, token) =>
                        {
                            AppCredentials.TrustServiceUrl(conversationReference.ServiceUrl);
                            await context.SendActivityAsync(message);
                        },
                        new CancellationToken());
            }
            catch (Exception ex)
            {
                await AddErrorMessageLog(conversationReference, message);
                await AddSystemLog(ex.Message);
                await SendAdminAsync(
                    $"Error happen in client with ConversationId: {conversationReference.Conversation.Id} & Message: {message}\n\n" +
                    $"Exception: {ex.Message}");
            }
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

        public async Task SendRequestAsync(string conversationId, long requestId, string messageText)
        {
            var conversation = await conversationService.GetConversationById(requestMessage.ConversationId);
            var connector = CreateConnectorClient(new Uri(conversation.ServiceUrl));
            var message = CreateMessageActivity(conversation);
            try
            {
                if (!string.IsNullOrEmpty(requestMessage.Message))
                {
                    message.Text = requestMessage.Message;
                    var cardActions = new List<CardAction>
                    {
                        new CardAction(ActionTypes.PostBack, "Approve", value: "cmdApprovedRequest:" + requestId.ToString()),
                        new CardAction(ActionTypes.PostBack, "Reject", value: "cmdRejectRequest:" + requestId.ToString())
                    };
                    var heroCard = new HeroCard
                    {
                        Buttons = cardActions
                    };

                    var attachment = heroCard.ToAttachment();
                    message.Attachments = new List<Attachment>()
                    {
                        attachment
                    };
                    await connector.Conversations.SendToConversationAsync((Activity)message);
                    await AddMessageLog(message);
                }
            }
            catch (Exception ex)
            {
                await AddErrorMessageLog(message);
                await AddSystemLog(ex.Message);
                await SendAdminAsync(
                    $"Error happen in client with ConversationId: {requestMessage.ConversationId} & Message: {requestMessage.Message}\n\n" +
                    $"Exception: {ex.Message}");
            }
        }

        public async Task SendAdminAsync(string message)
        {
            var conversations = await conversationService.GetAdminConversation();
            foreach (var conversation in conversations)
            {
                var adminConversationReferrence = GetConversationReference(conversation);
                await SendMessageAsync(adminConversationReferrence, message);
            }
        }

        public ConnectorClient CreateConnectorClient(Uri serviceUrl)
        {
            AppCredentials.TrustServiceUrl(serviceUrl.ToString(), DateTime.Now.AddDays(7));
            var account = new MicrosoftAppCredentials(configuration.GetSection("MicrosoftAppId").Value, configuration.GetSection("MicrosoftAppPassword").Value);
            var connector = new ConnectorClient(serviceUrl, account);

            return connector;
        }

        private static IMessageActivity CreateMessageActivity(ConversationModel conversation)
        {
            var message = Activity.CreateMessageActivity();
            message.ChannelId = conversation.ChannelId;
            message.From = new ChannelAccount
            {
                Id = conversation.ConversationBotId,
                Name = conversation.ConversationBotName
            };
            message.Recipient = new ChannelAccount
            {
                Id = conversation.ConversationId,
                Name = conversation.UserName
            };
            message.Conversation = new ConversationAccount(id: conversation.ConversationId);

            return message;
        }

        public async Task SendReplyAsync(IMessageActivity activity, string message)
        {
            var connector = CreateConnectorClient(new Uri(activity.ServiceUrl));
            var reply = (activity as Activity).CreateReply(message);
            reply.TextFormat = "markdown";
            await connector.Conversations.ReplyToActivityAsync(reply);
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

        private async Task AddErrorMessageLog(IMessageActivity activity)
        {
            var criteria = new InsertErrorMessageLogCriteria
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
    }
}