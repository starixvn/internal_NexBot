namespace NexBot.Services
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Bot.Connector;
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
        private readonly IConversationService conversationService;

        public MessageService(
            IConfiguration configuration,
            IConversationService conversationService)
        {
            this.configuration = configuration;
            this.conversationService = conversationService;
        }

        public async Task SendReplyAsync(IMessageActivity activity, string message)
        {
            var connector = CreateConnectorClient(new Uri(activity.ServiceUrl));
            var reply = (activity as Activity).CreateReply(message);
            reply.TextFormat = "markdown";

            await connector.Conversations.ReplyToActivityAsync(reply);
            await AddMessageLog(activity);

            await AddSystemLog("FromId: " + activity.From.Id + " & FromName: " + activity.From.Name
                + " & RecipientId: " + activity.Recipient.Id + " & RecipientName: " + activity.Recipient.Name);

            await AddSystemLog("ConversationId: " + activity.Conversation.Id
                + "ConversationName: " + activity.Conversation.Name
                + "IsGroup: " + activity.Conversation.IsGroup
                + "Properties: " + activity.Conversation.Properties

                + "Properties: " + activity.Conversation.Properties
                );
        }

        public async Task SendAdminAsync(string message)
        {
            var conversations = await conversationService.GetAdminConversation();
            foreach (var conversation in conversations)
            {
                var connector = CreateConnectorClient(new Uri(conversation.ServiceUrl));
                var adminMessage = CreateMessageActivity(conversation);
                adminMessage.Text = message;

                await connector.Conversations.SendToConversationAsync((Activity)adminMessage);
            }
        }

        public async Task SendAsync(string conversationId, string messageText)
        {
            await AddSystemLog("aa");
            var conversation = await conversationService.GetContactByConversationId(conversationId);
            var connector = CreateConnectorClient(new Uri(conversation.ServiceUrl));
            var message = CreateMessageActivity(conversation);
            await AddSystemLog("bb");
            try
            {
                if (!string.IsNullOrEmpty(messageText))
                {
                    message.Text = messageText;
                    await connector.Conversations.SendToConversationAsync((Activity)message);
                    await AddMessageLog(message);
                    await AddSystemLog("cc");
                }
            }
            catch (Exception ex)
            {
                await AddSystemLog("dd");
                await AddSystemLog(ex.Message);
                await SendAdminAsync(
                    $"Error happen in client with ConversationId: {conversationId} & Message: {messageText}\n\n" +
                    $"Exception: {ex.InnerException.Message}");
            }
        }

        public ConnectorClient CreateConnectorClient(Uri serviceUrl)
           => new ConnectorClient(
                serviceUrl,
                configuration.GetSection("MicrosoftAppId").Value,
                configuration.GetSection("MicrosoftAppPassword").Value);

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

        public async Task AddSystemLog(string logContent)
        {
            var criteria = new InsertSystemLogCriteria
            {
                LogContent = logContent
            };

            await ExecuteNonQueryAsync(criteria);
        }
    }
}