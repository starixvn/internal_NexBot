namespace NexBot.Channels
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Autofac;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Dialogs.Internals;
    using Microsoft.Bot.Connector;
    using Newtonsoft.Json.Linq;
    using NexBot.Core;
    using NexBot.Core.Utilities;
    using NexBot.Dialogs;
    using NexBot.Services.Interfaces;

    public class TelegramChannel : IChannel
    {
        private readonly IMessageService messageService;
        private readonly IConversationService conversationService;
        private readonly IRegisterMessageService registerMessageService;

        public TelegramChannel(IMessageService messageService,
             IConversationService conversationService,
             IRegisterMessageService registerMessageService)
        {
            this.messageService = messageService;
            this.conversationService = conversationService;
            this.registerMessageService = registerMessageService;
        }

        public async Task Post(Activity activity)
        {
            try
            {
                if (activity != null && activity.GetActivityType() == ActivityTypes.Message)
                {
                    var messageText = string.IsNullOrWhiteSpace(activity.Text) ? string.Empty : activity.Text;
                    if (messageText.StartsWith("/start"))
                    {
                        await messageService.AddSystemLog("Add contact: " + activity.From.Id);
                        await AddContact(activity);
                    }
                    else
                    {
                        await HandleMessageReceivedAsync(activity);
                    }
                }
                else
                {
                    await HandleSystemMessage(activity);
                }
            }
            catch (Exception ex)
            {
                await messageService.AddSystemLog(ex.Message);
            }
        }

        private async Task HandleMessageReceivedAsync(Activity activity)
        {
            try {
                var channelData = JObject.Parse(activity.ChannelData.ToString());
                JToken messageData;
                JToken newMember;

                messageData = channelData == null ? null : channelData["message"];
                newMember = messageData == null ? null : messageData["new_chat_member"];
                if (newMember != null)
                {
                    // add bot to group
                    await messageService.AddSystemLog("add to group");
                    await InsertConversationGroup(activity);
                }
                else
                {
                    var trimMessage = activity.Text.ToLowerInvariant().Trim();
                    if (activity.Conversation.IsGroup == true)
                    {
                        trimMessage = BotHelper.GetMessageInGroupByChannel(trimMessage, activity.ChannelId, activity.Recipient.Name.ToLowerInvariant());
                    }

                    if (trimMessage.Equals("help")
                        || trimMessage.Equals("register_events")
                        || trimMessage.Equals("remove_events"))
                    {
                        await messageService.AddSystemLog(trimMessage);
                        await Conversation.SendAsync(activity, () => new RegisterEventsDialog());
                    }
                    else if (trimMessage.StartsWith("register_"))
                    {
                        var eventCategories = await registerMessageService.GetAllCategoryMessage(activity.Conversation.Id);
                        if (eventCategories.Any(t => t.RegisterCommand.Contains(trimMessage)))
                        {
                            var categoryId = eventCategories.FirstOrDefault(t => t.RegisterCommand.Contains(trimMessage)).CategoryId;
                            var result = await registerMessageService.RegisterEventMessage(activity.Conversation.Id, categoryId);

                            await messageService.SendReplyAsync(activity, result.Message);
                        }
                    }
                    else if (trimMessage.StartsWith("remove_"))
                    {
                        var eventCategories = await registerMessageService.GetAllCategoryMessage(activity.Conversation.Id);

                        if (eventCategories.Any(t => t.RemoveCommand.Contains(trimMessage)))
                        {
                            var categoryId = eventCategories.FirstOrDefault(t => t.RemoveCommand.Contains(trimMessage)).CategoryId;
                            var result = await registerMessageService.RemoveRegisteredMessage(activity.Conversation.Id, categoryId);
                            await messageService.SendReplyAsync(activity, result.Message);
                        }
                    }
                    else if (trimMessage.StartsWith("help"))
                    {
                        var msg = $"List command: {Constants.NewLine}" +
                            $"**show_category_message:** List all category messages {Constants.NewLine}" +
                            $"**register_message `[categoryId]`:** Register to receive messages when an event triggered {Constants.NewLine}" +
                            $"**remove_registered_message `[categoryId]`:** Remove a registation receive message. After remove that, you cannot receive message when the event trigger {Constants.NewLine}";
                        await messageService.SendReplyAsync(activity, msg);
                    }
                    else
                    {
                        await messageService.SendReplyAsync(activity, $"Hello {activity.From.Name}, how are you today? {Constants.NewLine} If you need a help, please tag me with command *help*. Thanks.");
                    }
                }
            }
            catch (Exception ex)
            {
                await messageService.AddSystemLog(ex.Message);
            }
        }

        private async Task HandleSystemMessage(Activity activity)
        {
            switch (activity.Type)
            {
                case ActivityTypes.ConversationUpdate:
                    await messageService.AddSystemLog("add or remove member");
                    await HandleConversationUpdate(activity);
                    break;

                default:
                    break;
            }
        }

        private async Task AddMembersIntoGroup(Activity activity, JToken members)
        {
            string memberIds = string.Empty;

            await conversationService.AddMembersToGroup(activity.Conversation.Id, memberIds);
        }

        private async Task RemoveMembersOutGroup(Activity activity, JToken members)
        {
            string memberIds = string.Empty;

            await conversationService.AddMembersToGroup(activity.Conversation.Id, memberIds);
        }

        private async Task HandleConversationUpdate(Activity activity)
        {
            var channelData = JObject.Parse(activity.ChannelData.ToString());
            var messageData = channelData["message"];
            var newMembers = messageData["new_chat_members"];
            var leftMembers = messageData["left_chat_members"];

            await messageService.AddSystemLog("testing 1");

            if (newMembers != null)
            {
                await messageService.AddSystemLog("add members to group");
                await AddMembersIntoGroup(activity, newMembers);

            }
            else if (leftMembers != null)
            {
                await messageService.AddSystemLog("remove members out group");
                await RemoveMembersOutGroup(activity, leftMembers);
            }
        }

        private async Task InsertConversationGroup(Activity activity)
        {
            var conversation = await conversationService.GetConversationById(activity.Conversation.Id);

            if (conversation == null)
            {
                await conversationService.AddConversation(activity);
            }
        }

        private async Task AddContact(Activity activity)
        {
            await conversationService.AddConversation(activity);
            var message = $"Hello {activity.From.Name}. I am {activity.Recipient.Name}. If you have any question please contact admin.";
            await messageService.SendReplyAsync(activity, message);
        }
    }
}
