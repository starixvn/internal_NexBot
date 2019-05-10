namespace NexBot.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Autofac;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Dialogs.Internals;
    using Microsoft.Bot.Connector;
    using NexBot.Core;
    using NexBot.Core.Utilities;
    using NexBot.Dialogs;
    using NexBot.Models.CategoryMessage;
    using NexBot.Services.Interfaces;

    public class SkypeChannel : IChannel
    {
        private readonly IMessageService messageService;
        private readonly IRequestService requestService;
        private readonly IConversationService conversationService;
        private readonly IRegisterMessageService registerMessageService;

        public SkypeChannel(IMessageService messageService,
             IRequestService requestService,
             IConversationService conversationService,
             IRegisterMessageService registerMessageService)
        {
            this.messageService = messageService;
            this.requestService = requestService;
            this.conversationService = conversationService;
            this.registerMessageService = registerMessageService;
        }

        public async Task Post(Activity activity)
        {
            try
            {
                if (activity != null && activity.GetActivityType() == ActivityTypes.Message)
                {
                    await HandleMessageReceivedAsync(activity);
                }
                else
                {
                    await HandleSystemMessage(activity);
                }
            }
            catch (Exception ex)
            {
                await messageService.AddSystemLog(ex.Message);
                throw;
            }
            
        }

        private async Task<Activity> HandleContactRelationUpdate(Activity activity)
        {
            if (activity.Action.Equals("add"))
            {
                await conversationService.AddConversation(activity);
                var message = $"Hello {activity.From.Name}. I am {activity.Recipient.Name}. If you have any question please contact admin.";
                await messageService.SendReplyAsync(activity, message);
            }
            else
            {
                await conversationService.RemoveConversation(activity);
            }

            return null;
        }

        private async Task HandleMessageReceivedAsync(Activity activity)
        {
            try
            {
                IList<CategoryMessage> eventCategories;
                var trimMessage = activity.Text.Trim();
                
                if (activity.Conversation.IsGroup == true)
                {
                    trimMessage = BotHelper.GetMessageInGroupByChannel(trimMessage, activity.ChannelId, activity.Recipient.Name);
                }

                await Conversation.SendAsync(activity, () => new RegisterEventsDialog());
                eventCategories = await registerMessageService.GetAllCategoryMessage(activity.Conversation.Id);
                if (trimMessage.StartsWith("Remove"))
                {
                    if (eventCategories.Any(t => t.RemoveCommand.Equals(trimMessage)))
                    {
                        var categoryId = eventCategories.FirstOrDefault(t => t.RemoveCommand.Equals(trimMessage)).CategoryId;
                        var result = await registerMessageService.RemoveRegisteredMessage(activity.Conversation.Id, categoryId);
                        await messageService.SendReplyAsync(activity, result.Message);
                    }
                }
                else if (trimMessage.StartsWith("cmdApprovedRequest"))
                {
                    await requestService.ProcessRequest(activity.Conversation.Id, trimMessage, "Approve");
                }
                else if (trimMessage.StartsWith("cmdRejectRequest"))
                {
                    await requestService.ProcessRequest(activity.Conversation.Id, trimMessage, "Reject");
                }
                else if (trimMessage.StartsWith("getid"))
                {
                    await messageService.SendReplyAsync(activity, activity.Conversation.Id);
                }
                else if (trimMessage.StartsWith("stop_remind_reachedworkingtime"))
                {
                    var result = await registerMessageService.IgnoreEventMessage(activity.Conversation.Id, (int)MessageCategoryEnum.ReminderReachedWorkingHour);
                    if (result.IsSuccessful)
                    {
                        await messageService.SendReplyAsync(activity, result.Message);
                    }
                }
                else if (trimMessage.StartsWith("start_remind_reachedworkingtime"))
                {
                    var result = await registerMessageService.RemoveIgnoredEventMessage(activity.Conversation.Id, (int)MessageCategoryEnum.ReminderReachedWorkingHour);
                    if (result.IsSuccessful)
                    {
                        await messageService.SendReplyAsync(activity, result.Message);
                    }
                }
                else
                {
                    if (eventCategories.Any(t => t.RegisterCommand.Equals(trimMessage)))
                    {
                        var categoryId = eventCategories.FirstOrDefault(t => t.RegisterCommand.Equals(trimMessage)).CategoryId;
                        var result = await registerMessageService.RegisterEventMessage(activity.Conversation.Id, categoryId);

                        await messageService.SendReplyAsync(activity, result.Message);
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
                case ActivityTypes.DeleteUserData:
                    // Implement user deletion here
                    // If we handle user deletion, return a real message
                    break;

                case ActivityTypes.ConversationUpdate:
                    await HandleConversationUpdate(activity);
                    // Handle conversation state changes, like members being added and removed
                    // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                    // Not available in all channels
                    break;

                case ActivityTypes.ContactRelationUpdate:
                    await HandleContactRelationUpdate(activity);
                    // Handle add/remove from contact lists
                    // Activity.From + Activity.Action represent what happened
                    break;

                case ActivityTypes.Typing:
                    // Handle knowing that the user is typing
                    break;

                case ActivityTypes.Ping:
                    await messageService.SendReplyAsync(activity, "Pong");
                    break;

                default:
                    break;
            }
        }

        private async Task HandleConversationUpdate(Activity activity)
        {
            if (activity.MembersAdded != null && activity.MembersAdded.Count > 0)
            {
                await HandleAddMemberIntoGroup(activity);
            }
            else if (activity.MembersRemoved != null && activity.MembersRemoved.Count > 0)
            {
                await HandleRemoveMemberOutGroup(activity);
            }
        }

        private async Task HandleAddMemberIntoGroup(Activity activity)
        {
            if (activity.Conversation.IsGroup == true)
            {
                await conversationService.AddConversationGroup(activity);
            }

            var memberIds = string.Join(",", activity.MembersAdded.Select(m => m.Id));
            await conversationService.AddMembersToGroup(activity.Conversation.Id, memberIds);
            await messageService.AddSystemLog($"Added {memberIds} into group. {activity.Conversation.Id}");

            foreach (var member in activity.MembersAdded)
            {
                var isBot = await conversationService.CheckIsBot(member.Id);
                if (isBot)
                {
                    var memberExists = await GetAllMembersInfoInGroup(activity);
                    memberIds = string.Join(",", memberExists.Select(m => m.Id));
                    await conversationService.AddMembersToGroup(activity.Conversation.Id, memberIds);
                    await messageService.AddSystemLog($"Add bot to group with exists members: {memberIds}");
                }
            }
        }

        private async Task HandleRemoveMemberOutGroup(Activity activity)
        {
            var memberIds = string.Join(",", activity.MembersAdded.Select(m => m.Id));
            await conversationService.RemoveMembersOutGroup(activity.Conversation.Id, memberIds);
            await messageService.AddSystemLog($"Removed {memberIds} out group. {activity.Conversation.Id}");
        }

        private async Task<IList<ChannelAccount>> GetAllMembersInfoInGroup(Activity activity)
        {
            using (var scope = DialogModule.BeginLifetimeScope(Conversation.Container, activity))
            {
                var client = scope.Resolve<IConnectorClient>();
                var activityMembers = await client.Conversations.GetConversationMembersAsync(activity.Conversation.Id);
                return activityMembers;
            }
        }
    }
}
