namespace NexBot.Controllers
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Bot.Connector;
    using NexBot.Core.Utilities;
    using NexBot.Services.Interfaces;
    using NexBot.Utilitites;

    [Route("api/[controller]")]
    public class MessagesController : Controller
    {
        private readonly IMessageService messageService;
        private readonly IConversationService conversationService;

        public MessagesController(IMessageService messageService,
             IConversationService conversationService)
        {
            this.messageService = messageService;
            this.conversationService = conversationService;
        }

        [Authorize(Roles = "Bot")]
        [HttpPost]
        public async Task<OkResult> Post([FromBody] Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                await HandleMessageCommands(activity);
            }
            else
            {
                await HandleSystemMessage(activity);
            }

            return Ok();
        }

        [Authorize(Roles = "Bot")]
        [HttpPost]
        [Route("Forward")]
        public async Task Forward(string conversationId, string message)
        {
            await messageService.SendAsync(conversationId, message);
        }

        private async Task HandleMessageCommands(Activity activity)
        {
            if (BotHelper.GenerateMessage(activity.Text) == "get_info")
            {
                try
                {
                    var msg = $"GroupId: {activity.Conversation.Id}, Group Name : {activity.Name + " or " + activity.Conversation.Name}";
                    await messageService.SendAsync(activity.Conversation.Id, msg);
                    await conversationService.InsertGroupInfo(activity);
                    await messageService.SendAsync(activity.Conversation.Id, "Get info successful");
                }
                catch (Exception ex)
                {
                    await messageService.SendAsync(activity.Conversation.Id, "Error: " + ex.Message);
                }
            }
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
                var msg = $"IsGroup: {activity.Conversation.IsGroup.ToString() } " +
                    $"ConversationId: {activity.Conversation.Id} " +
                    $"Channel: {activity.ChannelId}" +
                    $"Server URL: {activity.ServiceUrl}" +
                    $"From Name: {activity.From.Name}. Ended";
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