namespace NexBot.Controllers
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;
    using NexBot.Core.Services.Interfaces;
    using NexBot.Core.Utilities;
    using NexBot.Services.Interfaces;

    [Route("api/[controller]")]
    public class MessagesController : Controller
    {
        private readonly IMessageService messageService;
        private readonly IConversationService conversationService;
        private readonly IBaseService baseService;
        private readonly IRegisterMessageService registerMessageService;

        public MessagesController(IMessageService messageService,
             IConversationService conversationService,
             IBaseService baseService,
             IRegisterMessageService registerMessageService)
        {
            this.messageService = messageService;
            this.conversationService = conversationService;
            this.baseService = baseService;
            this.registerMessageService = registerMessageService;
        }

        [Authorize(Roles = "Bot")]
        public virtual async Task<OkResult> Post([FromBody] Activity activity)
        {
            if (activity != null && activity.GetActivityType() == ActivityTypes.Message)
            {
                await HandleMessageReceivedAsync(activity);
            }
            else
            {
                await HandleSystemMessage(activity);
            }

            return Ok();
        }

        private async Task HandleMessageReceivedAsync(Activity activity)
        {
            var trimMessage = activity.Text.ToLowerInvariant().Trim();
            trimMessage = BotHelper.GenerateMessage(trimMessage);
            try
            {
                if (activity.Conversation.IsGroup == null)
                {
                    if (trimMessage.StartsWith("show_category_message"))
                    {
                        var msg = await registerMessageService.GetCategoryMessages(activity.Conversation.Id);
                        await messageService.SendReplyAsync(activity, msg);
                    }
                    else if (trimMessage.StartsWith("register_message"))
                    {
                        var categoryId = int.Parse(trimMessage.Substring("register_message".Length));
                        var result = await registerMessageService.RegisterEventMessage(activity.Conversation.Id, categoryId);

                        await messageService.SendReplyAsync(activity, result.Message);
                    }
                    else if (trimMessage.StartsWith("remove_registed_message"))
                    {
                        var categoryId = int.Parse(trimMessage.Substring("register_message".Length));
                        var result = await registerMessageService.RemoveRegisteredMessage(activity.Conversation.Id, categoryId);
                        await messageService.SendReplyAsync(activity, result.Message);
                    }
                }
                else
                {
                    await messageService.SendReplyAsync(activity, $"You said: {trimMessage}");
                }
            }
            catch (Exception ex)
            {
                await messageService.SendReplyAsync(activity, ex.Message);
            }
        }

        [Authorize(Roles = "Bot")]
        [HttpPost]
        [Route("Forward")]
        public async Task Forward(string conversationId, string message)
        {
            await messageService.SendAsync(conversationId, message);
        }

        private async Task<Activity> HandleSystemMessage(Activity activity)
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

            return null;
        }

        private async Task<Activity> HandleConversationUpdate(Activity activity)
        {
            if (activity.MembersAdded != null && activity.MembersAdded.Count > 0)
            {
                if (activity.Conversation.IsGroup == true)
                {
                    await InsertConversationGroup(activity);
                }

                foreach (var member in activity.MembersAdded)
                {
                    await conversationService.AddNewMemberToGroup(activity.Conversation.Id, member.Id);
                }

                var msg = string.Join(",", activity.MembersAdded.Select(m => m.Id));

                await messageService.SendReplyAsync(activity, msg);
            }
            else if (activity.MembersRemoved != null && activity.MembersRemoved.Count > 0)
            {
                foreach (var member in activity.MembersRemoved)
                {
                    await conversationService.RemoveMemberFromGroup(activity.Conversation.Id, member.Id);
                }

                await messageService.SendReplyAsync(activity, $"Removed {activity.MembersRemoved[0].Id} from group. {activity.Conversation.Id}");
            }

            return null;
        }

        private async Task InsertConversationGroup(Activity activity)
        {
            var conversation = await conversationService.GetContactByConversationId(activity.Conversation.Id);

            if (conversation == null)
            {
                await conversationService.AddContact(activity);
            }
        }

        private async Task<Activity> HandleContactRelationUpdate(Activity activity)
        {
            if (activity.Action.Equals("add"))
            {
                await conversationService.AddContact(activity);
                var message = $"Hello {activity.From.Name}. I am {activity.Recipient.Name}. If you have any question please contact admin.";
                await messageService.SendReplyAsync(activity, message);
            }
            else
            {
                await conversationService.RemoveContact(activity);
            }

            return null;
        }
    }
}