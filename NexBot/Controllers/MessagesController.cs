namespace NexBot.Controllers
{
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Bot.Connector;
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
            await messageService.SendAsync(conversationId, message, "https://smba.trafficmanager.net/apis");
        }

        private async Task HandleMessageCommands(Activity activity)
        {
            var message = "You said: " + activity.Text + ". Send to: " + activity.ServiceUrl + " - ChanelId: " + activity.ChannelId
                + " ConversationId: " + activity.Conversation.Id
                + " FromId: " + activity.From.Id
                + " FromName: " + activity.From.Name;
            await messageService.SendAsync(activity, message);
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
                    await messageService.SendAsync(activity, "Pong");
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
                var message = $"Added " + string.Join(", ", activity.MembersAdded.Select(x => x.Name)) + "  to group";
                await messageService.SendAsync(activity, message);
                //var result = await skypeUserService.AddMembersToGroup(activity.From.Id, activity.MembersAdded);

                //await ReplyMessage(activity, result.IsSuccessful ? "Save db success" : result.Message);
            }
            else if (activity.MembersRemoved != null && activity.MembersRemoved.Count > 0)
            {
                var message = $"Removed out group." + activity.MembersRemoved[0].Name;
                await messageService.SendAsync(activity, message);
                //var result = await skypeUserService.RemoveMembersFromGroup(activity.From.Id, activity.MembersRemoved);
                //await ReplyMessage(activity, result.IsSuccessful ? "Save db success" : result.Message);
            }

            return null;
        }

        private async Task<Activity> HandleContactRelationUpdate(Activity activity)
        {
            var message = $"Hello {activity.From.Name}, I'm Nexus Chat Bot. If you have any question please contact admin (bb8).";
            await messageService.SendAsync(activity, message);

            await conversationService.AddContact(activity);

            return null;
        }
    }
}