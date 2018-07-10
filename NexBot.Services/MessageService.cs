namespace NexBot.Utilitites
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Bot.Connector;
    using Microsoft.Extensions.Configuration;
    using NexBot.Models;

    public class MessageService : IMessageService
    {
        private readonly IConfiguration _configuration;

        public MessageService(
            IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendAsync(IMessageActivity activity, string message)
        {
            var connector = CreateConnectorClient(new Uri(activity.ServiceUrl));
            var reply = (activity as Activity).CreateReply(message);

            await connector.Conversations.ReplyToActivityAsync(reply);
        }

        //public async Task SendAdminAsync(string message)
        //{
        //    //var adminMessageInfos = _dbContext.MessageInfo.Where(messageInfo => messageInfo.IsAdmin);

        //    foreach (var adminMessageInfo in adminMessageInfos)
        //    {
        //        var connector = CreateConnectorClient(new Uri(adminMessageInfo.ServiceUrl));
        //        var adminMessage = CreateMessageActivity(adminMessageInfo);
        //        adminMessage.Text = message;

        //        await connector.Conversations.SendToConversationAsync((Activity)adminMessage);
        //    }
        //}

        public async Task SendAsync(string conversationId, string messageText, string serviceUrl)
        {
            //var messageInfo = await _dbContext
            //    .MessageInfo
            //    .FirstOrDefaultAsync(info => info.ConversationId == conversationId);
            var connector = CreateConnectorClient(new Uri(serviceUrl));
            var message = CreateMessageActivity(conversationId);

            try
            {
                if (!string.IsNullOrEmpty(messageText))
                {
                    message.Text = messageText;
                    await connector.Conversations.SendToConversationAsync((Activity)message);
                }
            }
            catch (Exception ex)
            {
                //await SendAdminAsync(
                //    $"Error happen in client {messageInfo?.ConversationId}\n\n" +
                //    $"Exception: {ex.InnerException.Message}");
            }
        }

        public ConnectorClient CreateConnectorClient(Uri serviceUrl)
           => new ConnectorClient(
                serviceUrl,
                _configuration.GetSection("MicrosoftAppId").Value,
                _configuration.GetSection("MicrosoftAppPassword").Value);

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

    }
}