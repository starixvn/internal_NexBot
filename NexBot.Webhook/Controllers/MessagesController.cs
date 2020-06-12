namespace NexBot.Controllers
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using NexBot.Services.Interfaces;

    [Route("api/[controller]")]
    public class MessagesController : Controller
    {
        private readonly IMessageService messageService;

        public MessagesController(IMessageService messageService)
        {
            this.messageService = messageService;
        }

        [Authorize(Roles = "Bot")]
        [HttpPost]
        [Route("Forward")]
        public async Task Forward(string conversationId, string message)
        {
            await messageService.SendAsync(conversationId, message);
        }

        [Route("ResendErrorMessage")]
        public async Task ResendErrorMessage()
        {
            await messageService.ReSendErrorMeesage();
        }
    }
}