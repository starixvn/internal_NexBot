namespace NexBot.Utilitites
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Bot.Connector;
    using Microsoft.Extensions.Configuration;
    using NexBot.Core.Services;
    using NexBot.Criteria.Message;
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

        public async Task SendAsync(IMessageActivity activity, string message)
        {
            var connector = CreateConnectorClient(new Uri(activity.ServiceUrl));
            var reply = (activity as Activity).CreateReply(message);

            await connector.Conversations.ReplyToActivityAsync(reply);
        }

        public async Task SendAdminAsync(string message)
        {
            var conversations = await conversationService.GetAdminConversation();
            foreach (var conversation in conversations)
            {
                var connector = CreateConnectorClient(new Uri(conversation.ServiceUrl));
                var adminMessage = CreateMessageActivity(conversation.ConversationId);
                adminMessage.Text = message;

                await connector.Conversations.SendToConversationAsync((Activity)adminMessage);
            }
        }

        public async Task SendAsync(string conversationId, string messageText)
        {
            var conversation = await conversationService.GetContactByConversationId(conversationId);
            var connector = CreateConnectorClient(new Uri(conversation.ServiceUrl));
            var message = CreateMessageActivity(conversationId);

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
                await SendAdminAsync(
                    $"Error happen in client with ConversationId: {conversationId} & Message: {messageText}\n\n" +
                    $"Exception: {ex.InnerException.Message}");
            }
        }

        public async Task SendAsync(string conversationId, string messageText, string serviceUrl)
        {
            var connector = CreateConnectorClient(new Uri(serviceUrl));
            var message = CreateMessageActivity(conversationId);

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

        //private static IMessageActivity CreateMessageActivity(MessageInfo messageInfo)
        //{
        //    var userAccount = new ChannelAccount(messageInfo.ToId, messageInfo.ToName);
        //    var botAccount = new ChannelAccount(messageInfo.FromId, messageInfo.FromName);
        //    var message = Activity.CreateMessageActivity();
        //    message.ChannelId = messageInfo.ChannelId;
        //    message.From = botAccount;
        //    message.Recipient = userAccount;
        //    message.Conversation = new ConversationAccount(id: messageInfo.ConversationId);

        //    return message;
        //}

        private static IMessageActivity CreateMessageActivity(string conversationId)
        {
            var message = Activity.CreateMessageActivity();
            //message.ChannelId = messageInfo.ChannelId;
            message.From = new ChannelAccount();
            message.Recipient = new ChannelAccount();
            message.Conversation = new ConversationAccount(id: conversationId);

            return message;
        }

        private async Task AddMessageLog(IMessageActivity activity)
        {
            var criteria = new InsertMessageLogCriteria
            {
                ConversationId = activity.Conversation.Id,
                FromConversationId = activity.From.Id,
                FromName = activity.From.Name,
                MessageContent = activity.Text
            };

            await ExecuteNonQueryAsync(criteria);
        }
    }
}