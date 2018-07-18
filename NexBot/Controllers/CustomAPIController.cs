using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NexBot.Models;
using NexBot.Services.Interfaces;

namespace NexBot.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomAPIController : Controller
    {
        private IMessageService messageService;

        public CustomAPIController(IMessageService messageService)
        {
            this.messageService = messageService;
        }

        //[Authorize(Roles = "Bot")]
        [HttpPost]
        public async Task<OkResult> SendMessage(SendMessageModel message)
        {
            await messageService.SendAsync(message.ConversationId, message.Message);
            return Ok();
        }
    }
}