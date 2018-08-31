namespace NexBot.Controllers
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Bot.Connector;
    using NexBot.Channels;
    using NexBot.Services.Interfaces;

    [Route("api/[controller]")]
    public class MessagesController : Controller
    {
        private readonly IMessageService messageService;
        private readonly IRequestService requestService;
        private readonly IConversationService conversationService;
        private readonly IRegisterMessageService registerMessageService;

        public MessagesController(IMessageService messageService,
             IRequestService requestService,
             IConversationService conversationService,
             IRegisterMessageService registerMessageService)
        {
            this.messageService = messageService;
            this.requestService = requestService;
            this.conversationService = conversationService;
            this.registerMessageService = registerMessageService;
        }

        [Authorize(Roles = "Bot")]
        public virtual async Task<OkResult> Post([FromBody] Activity activity)
        {
            IChannel channel;
            switch (activity.ChannelId)
            {
                case "skype":
                    channel = new SkypeChannel(messageService, requestService, conversationService, registerMessageService);
                    break;

                case "telegram":
                    channel = new TelegramChannel(messageService, conversationService, registerMessageService);
                    break;

                default:
                    channel = new SkypeChannel(messageService, requestService, conversationService, registerMessageService);
                    break;
            }

            await channel.Post(activity);

            return Ok();
        }

        [Authorize(Roles = "Bot")]
        [HttpPost]
        [Route("Forward")]
        public async Task Forward(string conversationId, string message)
        {
            await messageService.AddSystemLog("Forward");
            await messageService.SendAsync(conversationId, message);
        }
    }
}