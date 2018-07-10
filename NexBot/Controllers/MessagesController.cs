namespace NexBot.Controllers
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Bot.Connector;
    using Microsoft.Extensions.Configuration;
    using NexBot.Services.Interfaces;
    using NexBot.Utilitites;

    [Route("api/[controller]")]
    public class MessagesController : Controller
    {
        private readonly IMessageService messageService;
        private readonly IConfiguration configuration;
        private readonly IConversationService conversationService;

        public MessagesController(IMessageService messageService,
             IConfiguration configuration,
             IConversationService conversationService)
        {
            this.messageService = messageService;
            this.configuration = configuration;
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
            await ReplyMessage(activity, "You said: " + activity.Text + ". Send to: " + activity.ServiceUrl + " - ChanelId: " + activity.ChannelId
                + " ConversationId: " + activity.Conversation.Id
                + " FromId: " + activity.From.Id
                + " FromName: " + activity.From.Name
                );
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
                    await ReplyMessage(activity, "Pong");
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
                await ReplyMessage(activity, message);
                //var result = await skypeUserService.AddMembersToGroup(activity.From.Id, activity.MembersAdded);

                //await ReplyMessage(activity, result.IsSuccessful ? "Save db success" : result.Message);
            }
            else if (activity.MembersRemoved != null && activity.MembersRemoved.Count > 0)
            {
                var message = $"Removed out group." + activity.MembersRemoved[0].Name;
                await ReplyMessage(activity, message);
                //var result = await skypeUserService.RemoveMembersFromGroup(activity.From.Id, activity.MembersRemoved);
                //await ReplyMessage(activity, result.IsSuccessful ? "Save db success" : result.Message);
            }

            return null;
        }

        private async Task<Activity> HandleContactRelationUpdate(Activity activity)
        {
            var message = $"Hello {activity.From.Name}, I'm Nexus Chat Bot. If you have any question please contact admin (bb8).";
            await ReplyMessage(activity, message);
            //var result = await skypeUserService.AddContactWithBot(activity);
            //await ReplyMessage(activity, result.IsSuccessful ? "Save db success" : "Save db Failed");

            await conversationService.AddContact(activity);

            return null;
        }

        private async Task ReplyMessage(Activity activity, string message)
        {
            //var result = await conversation.SendAsync(activity.Conversation.Id, message);
            var appCredentials = new MicrosoftAppCredentials(this.configuration);
            var client = new ConnectorClient(new Uri(activity.ServiceUrl), appCredentials);
            var reply = activity.CreateReply(message);

            await client.Conversations.ReplyToActivityAsync(reply);
        }

        //private async Task SendMessage(string conversationId, string message)
        //{
        //    var appCredentials = new MicrosoftAppCredentials(this.configuration);
        //    var client = new ConnectorClient(new Uri(Request.Headers["host"]), appCredentials);
        //    var reply = activity.CreateReply(message);

        //    conversation.SendAsync(conversationId, message, Request.Headers["host"]);

        //    await client.Conversations.ReplyToActivityAsync(reply);
        //}
    }
}